using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class AttendanceDatabase(
    SqliteConnectionString connectionString,
    SignatureDatabase signatureDb,
    UserDatabase userDb)
    : SqliteDbBase(connectionString, signatureDb, userDb), IAttendanceDatabase
{
    public async Task<Attendance[]> Get(AttendanceQueryOptions options)
    {
        SelectQuery query = new();
        query.AppendLine($"""
            SELECT * FROM EventAttendances a
            LEFT JOIN AttendanceCompletions c
                ON a.{nameof(Attendance.CompletionId)} = c.{nameof(Completion.Id)}
            """);

        if (options.UserId is not null)
        {
            query.AppendConditionLine($"""
                {nameof(Attendance.UserId)} = @{nameof(options.UserId)}
                """);
        }
        if (options.LarpakeEventId is not null)
        {
            query.AppendConditionLine($"""
                {nameof(Attendance.LarpakeEventId)} = @{nameof(options.LarpakeEventId)}
                """);
        }
        if (options.IsCompleted is true)
        {
            query.AppendConditionLine($"""
                {nameof(Attendance.CompletionId)} IS NOT NULL
                """);
        }
        if (options.IsCompleted is false)
        {
            query.AppendConditionLine($"""
                {nameof(Attendance.CompletionId)} IS NULL
                """);
        }
        if (options.After is not null)
        {
            query.AppendConditionLine($"""
                {nameof(Attendance.CreatedAt)} >= @{nameof(options.After)}
                """);
        }
        if (options.Before is not null)
        {
            query.AppendConditionLine($"""
                {nameof(Attendance.CreatedAt)} <= @{nameof(options.Before)}
                """);
        }
        query.AppendLine($"""
            ORDER BY {nameof(Attendance.LarpakeEventId)} ASC, {nameof(Attendance.CompletionId)} ASC NULLS LAST 
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)};
            """);

        using var connection = await GetConnection();
        var records = await connection.QueryAsync<Attendance, Completion?, Attendance>(query.ToString(),
            (attendance, completion) =>
            {
                attendance.Completion = completion;
                return attendance!;
            },
            options,
            splitOn: nameof(Completion.Id));

        return records.ToArray();
    }

    public async Task<Result<int>> InsertUncompleted(Attendance attendance)
    {
        using var connection = await GetConnection();
        try
        {
            return await connection.ExecuteAsync($"""
                INSERT INTO EventAttendances (
                    {nameof(Attendance.UserId)}, 
                    {nameof(Attendance.LarpakeEventId)}, 
                    {nameof(Attendance.CompletionId)}
                )
                VALUES (
                    @{nameof(attendance.UserId)},
                    @{nameof(attendance.LarpakeEventId)},
                    NULL
                );
                """, attendance);
        }
        catch (SqliteException ex)
        {
            switch (ex)
            {
                case { SqliteExtendedErrorCode: SqliteError.PrimaryKey_e }:
                    return Error.Conflict("Attendance already exists", ex);
                case { SqliteExtendedErrorCode: SqliteError.ForeignKey_e }:
                    return Error.NotFound("Attendance already exists", ex);
                default:
                    throw;
            }
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

            string query = $"""
                DELETE FROM AttendanceCompletions
                    WHERE {nameof(Completion.Id)} IN (
                        SELECT {nameof(Attendance.CompletionId)} FROM EventAttendances
                        WHERE {nameof(Attendance.UserId)} = @{nameof(userId)}
                            AND {nameof(Attendance.LarpakeEventId)} = @{nameof(eventId)}
                );

                UPDATE EventAttendances 
                SET
                    {nameof(Attendance.CompletionId)} = NULL,
                    {nameof(Attendance.UpdatedAt)} = DATETIME('now')
                WHERE {nameof(Attendance.UserId)} = @{nameof(userId)}
                    AND {nameof(Attendance.LarpakeEventId)} = @{nameof(eventId)};
                """;
            using var connection = await GetConnection();
            return await connection.ExecuteAsync(query, new { userId, eventId });
        }
        catch (SqliteException ex)
        {
            switch (ex)
            {
                case { SqliteExtendedErrorCode: SqliteError.ForeignKey_e }:
                    return Error.NotFound("Attendance already exists", ex);
                default:
                    throw;
            }
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
            completion.Id = Guid.CreateVersion7();

            using var connection = await GetConnection();

            // This query inserts AttendanceCompletion only if event
            // attendance with userId and eventId exists
            var (completionId, attendanceExists) = await connection.QueryFirstOrDefaultAsync<(Guid? Id, bool RecordExists)>($"""
                SELECT
                    {nameof(Attendance.CompletionId)} AS Id,
                    TRUE AS RecordExists
                FROM EventAttendances
                WHERE {nameof(Attendance.UserId)} = @{nameof(completion.UserId)}
                    AND {nameof(Attendance.LarpakeEventId)} = @{nameof(completion.EventId)}
                LIMIT 1;
                """, completion);

            if (attendanceExists && completionId is not null)
            {
                // If attendance is already completed
                return DataError.AlreadyExistsNoError(
                    completionId.Value, 
                    nameof(AttendedCreated.CompletionId),
                    $"Attendance is already completed, completion id in response body.");
            }

            if (attendanceExists is false)
            {
                return Error.NotFound("Attendance with given userId and eventId not found");
            }

            int rowsAffected = await connection.ExecuteAsync($"""
                INSERT INTO AttendanceCompletions (
                    {nameof(Completion.Id)}, 
                    {nameof(Completion.SignerId)}, 
                    {nameof(Completion.CompletedAt)}
                )
                VALUES (
                    @{nameof(completion.Id)},
                    @{nameof(completion.SignerId)},
                    @{nameof(completion.CompletedAt)}
                ); 
                
                UPDATE EventAttendances 
                SET
                    {nameof(Attendance.CompletionId)} = @{nameof(completion.Id)}, 
                    {nameof(Attendance.UpdatedAt)} = DATETIME('now')
                WHERE {nameof(Attendance.UserId)} = @{nameof(completion.UserId)}
                    AND {nameof(Attendance.LarpakeEventId)} = @{nameof(completion.EventId)};
                """, completion);

            return new AttendedCreated
            {
                CompletionId = completion.Id,
                LarpakeEventId = completion.EventId,
                UserId = completion.UserId,
            };
        }
        catch (SqliteException ex)
        {
            switch (ex)
            {
                case { SqliteExtendedErrorCode: SqliteError.ForeignKey_e }:
                    return Error.NotFound("Attendance does not exist.", ex);
                case { SqliteExtendedErrorCode: SqliteError.PrimaryKey_e }:
                    return Error.InternalServerError("Cannot create valid UUID, try again.", ex);
                default:
                    throw;
            }
        }
    }

    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS AttendanceCompletions (
                {nameof(Completion.Id)} TEXT,
                {nameof(Completion.SignerId)} TEXT NOT NULL,
                {nameof(Completion.SignatureId)} TEXT,
                {nameof(Completion.CompletedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Completion.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Completion.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(Completion.Id)}),
                FOREIGN KEY ({nameof(Completion.SignerId)}) REFERENCES Users({nameof(User.Id)}),
                FOREIGN KEY ({nameof(Completion.SignatureId)}) REFERENCES Signatures({nameof(Signature.Id)})
            );

            CREATE TABLE IF NOT EXISTS EventAttendances (
                {nameof(Attendance.UserId)} TEXT,
                {nameof(Attendance.LarpakeEventId)} INTEGER,
                {nameof(Attendance.CompletionId)} TEXT,
                {nameof(Attendance.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Attendance.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(Attendance.UserId)}, {nameof(Attendance.LarpakeEventId)}),
                FOREIGN KEY ({nameof(Attendance.UserId)}) REFERENCES Users({nameof(User.Id)}),
                FOREIGN KEY ({nameof(Attendance.LarpakeEventId)}) REFERENCES Events({nameof(OrganizationEvent.Id)}),
                FOREIGN KEY ({nameof(Attendance.CompletionId)}) REFERENCES AttendanceCompletions({nameof(Completion.Id)})
            );
            """);
    }

    public Task<Result<AttendanceKey>> GetAttendanceKey(Attendance attendance)
    {
        throw new NotImplementedException();
    }

    public Task<Result<AttendedCreated>> CompletedKeyed(KeyedCompletionMetadata completion)
    {
        throw new NotImplementedException();
    }

    Task<int> IAttendanceDatabase.Clean()
    {
        throw new NotImplementedException();
    }
}
