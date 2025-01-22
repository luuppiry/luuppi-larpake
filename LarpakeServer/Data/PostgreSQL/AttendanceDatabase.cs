using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class AttendanceDatabase : PostgresDb, IAttendanceDatabase
{
    readonly AttendanceKeyService _keyService;

    public AttendanceDatabase(
        NpgsqlConnectionString connectionString,
        ILogger<AttendanceDatabase> logger,
        AttendanceKeyService keyService)
        : base (connectionString, logger)
    {
        _keyService = keyService;
    }


    public async Task<Attendance[]> Get(AttendanceQueryOptions options)
    {
        SelectQuery query = new();
        query.AppendLine($"""
            SELECT * FROM attendances a
            LEFT JOIN completions c
                ON a.completion_id = c.id
            """);

        // Search specific user
        query.IfNotNull(options.UserId).AppendConditionLine($"""
            a.user_id = @{nameof(options.UserId)}
            """);

        // Search specific event
        query.IfNotNull(options.LarpakeEventId).AppendConditionLine($"""
            a.larpake_event_id = @{nameof(options.LarpakeEventId)}
            """);

        // Only completed
        query.IfTrue(options.IsCompleted).AppendConditionLine($"""
            c.id IS NOT NULL
            """);

        // Only uncompleted
        query.IfFalse(options.IsCompleted).AppendConditionLine($"""
            c.id IS NULL
            """);

        // Attendance created after specific date
        query.IfNotNull(options.After).AppendConditionLine($"""
            a.created_at >= @{nameof(options.After)} 
            """);
        
        // Attendance created before specific date
        query.IfNotNull(options.Before).AppendConditionLine($"""
            a.created_at <= @{nameof(options.After)} 
            """);

        // Completed after specific date
        query.IfNotNull(options.CompletedAfter).AppendConditionLine($"""
            c.completed_at >= @{nameof(options.After)} 
            """);
        
        // Completed before specific date
        query.IfNotNull(options.CompletedBefore).AppendConditionLine($"""
            c.completed_at <= @{nameof(options.After)} 
            """);

        query.AppendLine($"""
            ORDER BY a.larpake_event_id ASC, c.completed_at DESC NULLS LAST
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)}
            """);

        using var connection = GetConnection();
        var records = await connection.QueryAsync<Attendance, Completion, Attendance>(
            query.ToString(), 
            (attendance, completion) =>
            {
                attendance.Completion = completion;
                return attendance;
            },
            options,
            splitOn: "id");

        return records.ToArray();
    }

    public async Task<Result<AttendanceKey>> RequestAttendanceKey(Attendance attendance)
    {
        AttendanceKey key = _keyService.GenerateKey();
        attendance.Key = key.Value;
        attendance.KeyInvalidAt = key.InvalidAt;

        try
        {
            using var connection = GetConnection();
            key = await connection.QueryFirstAsync<AttendanceKey>($"""
                INSERT INTO attendances (
                    user_id, 
                    larpake_event_id,
                    completion_id,
                    key,
                    key_invalid_at
                )
                VALUES (
                    @{nameof(attendance.UserId)}, 
                    @{nameof(attendance.LarpakeEventId)}, 
                    NULL,
                    @{nameof(attendance.Key)},
                    @{nameof(attendance.KeyInvalidAt)}
                )
                ON CONFLICT (user_id, larpake_event_id) 
                    DO UPDATE
                    SET 
                        key_invalid_at = @{nameof(attendance.KeyInvalidAt)}
                    RETURNING key, key_invalid_at;
                """, attendance);
            return key;
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError(ex, "Failed to insert uncompleted attendance");
            throw;
        }
    }


    public async Task<Result<AttendedCreated>> CompletedKeyed(KeyedCompletionMetadata completion)
    {
        if (string.IsNullOrWhiteSpace(completion.Key))
        {
            return Error.BadRequest("Key cannot be null or empty.");
        }
        if (completion.SignerId == Guid.Empty)
        {
            return Error.BadRequest("SignerId cannot be null.");
        }


        try
        {
            using var connection = GetConnection();

            // Get by key
            var attendance = await connection.QueryFirstOrDefaultAsync<Attendance>($"""
                SELECT 
                    user_id,
                    larpake_event_id,
                    completion_id,
                FROM attendances
                    WHERE key = @{nameof(completion.Key)}
                        AND key_invalid_at > NOW();
                """);

            if (attendance is null)
            {
                return Error.NotFound("Attendance with given key not found or key expired.");
            }

            if (attendance.CompletionId is not null)
            {
                return DataError.AlreadyExistsNoError(
                    attendance.CompletionId.Value, nameof(AttendedCreated.CompletionId),
                    $"Attendance is already completed, completion id in response body.");
            }

            var record = new Completion
            {
                Id = Guid.CreateVersion7(),
                SignerId = completion.SignerId,
                SignatureId = completion.SignatureId,
                CompletedAt = completion.CompletedAt
            };

            await connection.ExecuteAsync($"""
                INSERT INTO completions (
                    id,
                    signer_id,
                    signature_id,
                    completed_at,
                )
                VALUES (
                    @{nameof(record.Id)},
                    @{nameof(record.SignerId)},
                    @{nameof(record.SignatureId)},
                    NOW()
                );

                UPDATE attendances
                SET 
                    completion_id = @{nameof(record.Id)}
                    updated_at = NOW()
                WHERE key = @{nameof(completion.Key)};
                """, record);

            return new AttendedCreated
            {
                CompletionId = record.Id,
                LarpakeEventId = attendance.LarpakeEventId,
                UserId = attendance.UserId
            };


        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError(ex, "Failed to complete attendance.");
            throw;
        }
    }

    public async Task<Result<AttendedCreated>> Complete(CompletionMetadata completion)
    {
        if (completion.UserId == Guid.Empty)
        {
            return Error.BadRequest("UserId cannot be null.");
        }
        if (completion.EventId is Constants.NullId)
        {
            return Error.BadRequest("EventId cannot be -1.");
        }
        if (completion.SignerId == Guid.Empty)
        {
            return Error.BadRequest("SignerId cannot be null.");
        }

        try
        {
            using var connection = GetConnection();

            /* This query inserts completion only if
             * attendance with userId and eventId exists
             */

            var (oldId, oldExists) = await connection.QueryFirstOrDefaultAsync<(Guid? Id, bool Exists)>($"""
                SELECT 
                    completion_id AS Id,
                    TRUE as Exists
                FROM attendances
                WHERE user_id = @{nameof(completion.UserId)}
                    AND larpake_event_id = @{nameof(completion.EventId)}
                LIMIT 1;
                """, completion);

            if (oldExists is false)
            {
                return Error.NotFound("Attendance with given userId and eventId not found");
            }
            if (oldId is not null)
            {
                return DataError.AlreadyExistsNoError(
                    oldId.Value, nameof(AttendedCreated.CompletionId),
                    $"Attendance is already completed, completion id in response body.");
            }

            completion.Id = Guid.CreateVersion7();
            
            await connection.ExecuteAsync($"""
                INSERT INTO completions (
                    id,
                    signer_id,
                    signature_id,
                    completed_at,
                    updated_at
                )
                VALUES (
                    @{nameof(completion.Id)},
                    @{nameof(completion.SignerId)},
                    @{nameof(completion.SignatureId)},
                    @{nameof(completion.CompletedAt)},
                    NOW()
                );

                UPDATE attendances
                SET 
                    completion_id = @{nameof(completion.Id)}
                    updated_at = NOW()
                WHERE user_id = @{nameof(completion.UserId)}
                    AND larpake_event_id = @{nameof(completion.EventId)};
                """, completion);

            return new AttendedCreated
            {
                CompletionId = completion.Id,
                LarpakeEventId = completion.EventId,
                UserId= completion.UserId
            };
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError(ex, "Failed to complete attendance.");
            throw;
        }
    }

    public async Task<Result<int>> Uncomplete(Guid userId, long eventId)
    {
        if (userId == Guid.Empty)
        {
            return Error.BadRequest("UserId cannot be empty.");
        }
        if (eventId is Constants.NullId)
        {
            return Error.BadRequest("EventId cannot be -1.");
        }

        try
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync($"""
                UPDATE attendances
                SET completion_id = NULL
                WHERE completion_id IN (
                    DELETE FROM completions
                    WHERE user_id = @{nameof(userId)} 
                        AND larpake_event_id = @{nameof(eventId)}
                    RETURNING id
                );
                """, new { userId, eventId });
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError(ex, "Failed to uncomplete attendance.");
            throw;
        }
    }
}
