using Dapper;
using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class AttendanceDatabase : PostgresDb, IAttendanceDatabase
{
    readonly AttendanceKeyService _keyService;
    readonly ConflictRetryPolicyOptions _retryPolicy;

    public AttendanceDatabase(
        NpgsqlConnectionString connectionString,
        ILogger<AttendanceDatabase> logger,
        AttendanceKeyService keyService,
        IOptions<ConflictRetryPolicyOptions> retryPolicy)
        : base(connectionString, logger)
    {
        _keyService = keyService;
        _retryPolicy = retryPolicy.Value;
    }


    public async Task<Attendance[]> Get(AttendanceQueryOptions options)
    {
        SelectQuery query = new();
        query.AppendLine($"""
            SELECT
                a.user_id,
                a.larpake_event_id AS larpake_task_id,
                a.completion_id,
                a.created_at,
                a.updated_at,
                a.qr_code_key,
                a.key_invalid_at,
                c.id,
                c.signer_id,
                c.signature_id,
                c.completed_at,
                c.created_at,
                c.updated_at
            FROM attendances a
            LEFT JOIN completions c
                ON a.completion_id = c.id
            """);

        query.IfNotNull(options.LarpakeId).AppendLine($"""
            LEFT JOIN larpake_events e
                ON a.larpake_event_id = e.id
            LEFT JOIN larpake_sections s
                ON e.larpake_section_id = s.id
            """).AppendConditionLine($"""
            s.larpake_id = @{nameof(options.LarpakeId)}
            """);

        // Search specific user
        query.IfNotNull(options.UserId).AppendConditionLine($"""
            a.user_id = @{nameof(options.UserId)}
            """);

        // Search specific event
        query.IfNotNull(options.LarpakeTaskId).AppendConditionLine($"""
            a.larpake_event_id = @{nameof(options.LarpakeTaskId)}
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

        string q = query.ToString();

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

    public async Task<Attendance?> GetByKey(string key)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<Attendance, Completion, Attendance>($"""
            SELECT
                a.user_id,
                a.larpake_event_id AS larpake_task_id,
                a.completion_id,
                a.created_at,
                a.updated_at,
                a.qr_code_key,
                a.key_invalid_at,
                c.id,
                c.signer_id,
                c.signature_id,
                c.completed_at,
                c.created_at,
                c.updated_at
            FROM attendances a
            LEFT JOIN completions c
                ON a.completion_id = c.id
            WHERE a.qr_code_key = @{nameof(key)}
            LIMIT 1;
            """,
            (attendance, completion) =>
            {
                attendance.Completion = completion;
                return attendance;
            },
            new { key }, splitOn: "id");

        return records.FirstOrDefault();
    }

    public async Task<Result<AttendanceKey>> GetAttendanceKey(Attendance attendance)
    {
        /* Attendance can be created if all true:
         * - User exists
         * - User is participating in this Lärpäke
         * - User's is_competing == true
         */

        /* Action is Retried
         * - key generation conflict might appear (very unlikely, 33^6 and 33^8 are big numbers).
         * - Generates key if key is not already generated
         * - If key already exists refreshes invalidation date and returns existing key
         */

        for (int retriesLeft = _retryPolicy.MaxRetries; retriesLeft > 0; retriesLeft--)
        {
            AttendanceKey key = _keyService.GenerateKey();
            attendance.QrCodeKey = key.QrCodeKey;
            attendance.KeyInvalidAt = key.KeyInvalidAt;

            try
            {
                using var connection = GetConnection();
                IEnumerable<bool> competingStatuses = await connection.QueryAsync<bool>($"""
                     SELECT 
                        m.is_competing 
                    FROM larpake_events e
                        LEFT JOIN larpake_sections s
                            ON e.larpake_section_id = s.id
                        LEFT JOIN freshman_groups g
                            ON s.larpake_id = g.larpake_id
                        LEFT JOIN freshman_group_members m 
                            ON g.id = m.group_id
                    WHERE e.id = @{nameof(attendance.LarpakeTaskId)}
                        AND m.user_id = @{nameof(attendance.UserId)};
                    """, attendance);

                bool[] statuses = competingStatuses.ToArray();

                // User and event not found
                if (statuses.Length <= 0)
                {
                    return Error.BadRequest("User not attending given larpake.",
                        ErrorCode.UserNotAttending);
                }

                // User and event found, but user is tutor and cannot attend
                if (statuses.All(x => x is false))
                {
                    return Error.Forbidden("User has non competing status.",
                        ErrorCode.UserStatusTutor);
                }

                key = await connection.QueryFirstAsync<AttendanceKey>($"""
                INSERT INTO attendances (
                    user_id, 
                    larpake_event_id,
                    completion_id,
                    qr_code_key,
                    key_invalid_at
                )
                VALUES (
                    @{nameof(attendance.UserId)}, 
                    @{nameof(attendance.LarpakeTaskId)}, 
                    NULL,
                    @{nameof(attendance.QrCodeKey)},
                    @{nameof(attendance.KeyInvalidAt)}
                )
                ON CONFLICT (user_id, larpake_event_id) 
                    DO UPDATE
                    SET 
                        key_invalid_at = @{nameof(attendance.KeyInvalidAt)}
                RETURNING qr_code_key, key_invalid_at;
                """, attendance);
                return key;
            }
            catch (PostgresException ex) when (ex.SqlState is PostgresError.UniqueViolation)
            {
                Logger.LogInformation("QrCodeKey conflict for {hash}, retrying ({count} left).",
                    attendance.GetHashCode(), retriesLeft);
                continue;

            }
            catch (Exception ex)
            {
                // TODO: Handle exception
                Logger.LogError(ex, "Failed to insert uncompleted attendance");
                throw;
            }
        }
        return Error.InternalServerError("Key generation failed, retry with same parameters.", ErrorCode.KeyGenFailed);
    }

    public async Task<Result<AttendedCreated>> CompletedKeyed(KeyedCompletionMetadata completion)
    {
        /* Requirements to successfully complete:
         * - Key and exits
         * - Key is not expired
         * - Signer must also attend same Larpake
         * - Signer is not same as completion user
         * - Signer is with is_competing == FALSE status
         */

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
            /* About key invalidation:
             * - Doing this in transaction to ensure that the key is only 
             *      invalidated if successfully completed
             * - Key is not deleted, because we don't want same key used immidiately.
             * - Key should be deleted after cooldown period (like 5 days).
             */
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            // Get attendance, Note that signer cannot get attendance with same userId as signerId 
            var attendance = await connection.QueryFirstOrDefaultAsync<Attendance>($"""
                UPDATE attendances
                    SET 
                        key_invalid_at = NOW(),
                        updated_at = NOW()
                WHERE qr_code_key = @{nameof(completion.Key)}
                    AND key_invalid_at > NOW()
                RETURNING
                    user_id,
                    larpake_event_id,
                    completion_id;
                """, completion);


            if (attendance is null)
            {
                return Error.NotFound("Attendance with given key not found or key expired.", ErrorCode.IdNotFound);
            }
            if (attendance.UserId == completion.SignerId)
            {
                return Error.BadRequest("Cannot self-sign attendance", ErrorCode.CannotSignSelf);
            }
            if (attendance.CompletionId is not null)
            {
                transaction.Commit();
                return DataError.AlreadyExistsNoError(
                    attendance.CompletionId.Value,
                    nameof(AttendedCreated.CompletionId),
                    $"Attendance is already completed, completion id in response body.");
            }

            // Validate signer is in same Larpake and is not competing
            bool isSignerValid = await connection.ExecuteScalarAsync<bool>($"""
                SELECT CanUserSignAttendance(@{nameof(completion.SignerId)}, @{nameof(attendance.LarpakeTaskId)});
                """, new { completion.SignerId, attendance.LarpakeTaskId });

            if (isSignerValid is false)
            {
                return Error.BadRequest("Signer must attend larpake event is part of.");
            }

            var record = new Completion
            {
                Id = Guid.CreateVersion7(),
                SignerId = completion.SignerId,
                CompletedAt = completion.CompletedAt
            };

            // Create completion
            await connection.ExecuteAsync($"""
                INSERT INTO completions (
                    id,
                    signer_id,
                    signature_id,
                    completed_at
                )
                VALUES (
                    @{nameof(record.Id)},
                    @{nameof(record.SignerId)},
                    (
                        SELECT id FROM signatures
                        WHERE user_id = @{nameof(record.SignerId)}
                        ORDER BY RANDOM() LIMIT 1
                    ),
                    NOW()
                );
                """, record);

            // Update attendance to completed
            await connection.ExecuteAsync($"""
                UPDATE attendances
                SET 
                    completion_id = @{nameof(record.Id)},
                    updated_at = NOW(),
                    key_invalid_at = NOW()
                WHERE 
                    qr_code_key = @{nameof(completion.Key)};
                """, new { record.Id, completion.Key });

            await transaction.CommitAsync();

            Logger.LogInformation("User {userId} completed event {eventId}.",
                attendance.UserId, attendance.LarpakeTaskId);

            return new AttendedCreated
            {
                CompletionId = record.Id,
                LarpakeEventId = attendance.LarpakeTaskId,
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
        /* Requirements to successfully complete:
         * - User, Signer and Event exists
         * - Attendance is already created with given userId and eventId (This prevents invalid attendances from users e.g. no competing)
         * - Event is not already completed (if completed -> returns completed id, no error)
         */

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

            /* This query inserts completion only if attendance with userId and eventId exists
             * - This method does not check if signer attends the larpake event is part of
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
                    (
                        SELECT id FROM signatures
                        WHERE user_id = @{nameof(completion.SignerId)}
                        ORDER BY RANDOM() LIMIT 1
                    ),
                    @{nameof(completion.CompletedAt)},
                    NOW()
                );

                UPDATE attendances
                SET 
                    completion_id = @{nameof(completion.Id)},
                    updated_at = NOW(),
                    key_invalid_at = NOW()
                WHERE user_id = @{nameof(completion.UserId)}
                    AND larpake_event_id = @{nameof(completion.EventId)};
                """, completion);

            Logger.LogInformation("User {userId} completed event {eventId}.",
                completion.UserId, completion.EventId);

            return new AttendedCreated
            {
                CompletionId = completion.Id,
                LarpakeEventId = completion.EventId,
                UserId = completion.UserId
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

    public async Task<int> Clean()
    {
        // Call stored procedure
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>("SELECT CleanAttendanceKeys();");
    }
}
