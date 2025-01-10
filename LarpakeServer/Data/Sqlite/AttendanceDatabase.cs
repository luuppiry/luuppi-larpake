using LarpakeServer.Helpers.Generic;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Models.QueryOptions;
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
                ON a.{nameof(Attendance.CompletionId)} = c.{nameof(AttendanceCompletion.Id)}
            """);

        if (options.UserId is not null)
        {
            query.AppendConditionLine($"""
                {nameof(Attendance.UserId)} = @{nameof(options.UserId)}
                """);
        }
        if (options.EventId is not null)
        {
            query.AppendConditionLine($"""
                {nameof(Attendance.EventId)} = @{nameof(options.EventId)}
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
            ORDER BY {nameof(Attendance.EventId)} ASC, {nameof(Attendance.CompletionId)} ASC NULLS LAST 
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)};
            """);

        string str = query.ToString();
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<Attendance, AttendanceCompletion?, Attendance>(query.ToString(),
            (attendance, completion) =>
            {
                attendance.Completion = completion;
                return attendance!;
            },
            options,
            splitOn: nameof(AttendanceCompletion.Id));

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
                    {nameof(Attendance.EventId)}, 
                    {nameof(Attendance.CompletionId)}
                )
                VALUES (
                    @{nameof(attendance.UserId)},
                    @{nameof(attendance.EventId)},
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
                    WHERE {nameof(AttendanceCompletion.Id)} IN (
                        SELECT {nameof(Attendance.CompletionId)} FROM EventAttendances
                        WHERE {nameof(Attendance.UserId)} = @{nameof(userId)}
                            AND {nameof(Attendance.EventId)} = @{nameof(eventId)}
                );

                UPDATE EventAttendances 
                SET
                    {nameof(Attendance.CompletionId)} = NULL,
                    {nameof(Attendance.UpdatedAt)} = DATETIME('now')
                WHERE {nameof(Attendance.UserId)} = @{nameof(userId)}
                    AND {nameof(Attendance.EventId)} = @{nameof(eventId)};
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

    public async Task<Result<AttendedCreated>> Complete(AttendanceCompletionMetadata completion)
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

            // This query inserts AttendanceCompletion only id event
            // attendance with userId and eventId exists
            var (completionId, attendanceExists) = await connection.QueryFirstOrDefaultAsync<(Guid? Id, bool RecordExists)>($"""
                SELECT
                    {nameof(Attendance.CompletionId)} AS Id,
                    TRUE AS RecordExists
                FROM EventAttendances
                WHERE {nameof(Attendance.UserId)} = @{nameof(completion.UserId)}
                    AND {nameof(Attendance.EventId)} = @{nameof(completion.EventId)}
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
                    {nameof(AttendanceCompletion.Id)}, 
                    {nameof(AttendanceCompletion.SignerId)}, 
                    {nameof(AttendanceCompletion.SignatureId)},
                    {nameof(AttendanceCompletion.CompletionTimeUtc)}
                )
                VALUES (
                    @{nameof(completion.Id)},
                    @{nameof(completion.SignerId)},
                    @{nameof(completion.SignatureId)},
                    @{nameof(completion.CompletionTimeUtc)}
                ); 
                
                UPDATE EventAttendances 
                SET
                    {nameof(Attendance.CompletionId)} = @{nameof(completion.Id)}, 
                    {nameof(Attendance.UpdatedAt)} = DATETIME('now')
                WHERE {nameof(Attendance.UserId)} = @{nameof(completion.UserId)}
                    AND {nameof(Attendance.EventId)} = @{nameof(completion.EventId)};
                """, completion);

            return new AttendedCreated
            {
                CompletionId = completion.Id,
                EventId = completion.EventId,
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
                {nameof(AttendanceCompletion.Id)} TEXT,
                {nameof(AttendanceCompletion.SignerId)} NOT NULL,
                {nameof(AttendanceCompletion.SignatureId)} TEXT,
                {nameof(AttendanceCompletion.CompletionTimeUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(AttendanceCompletion.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(AttendanceCompletion.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(AttendanceCompletion.Id)}),
                FOREIGN KEY ({nameof(AttendanceCompletion.SignerId)}) REFERENCES Users({nameof(User.Id)}),
                FOREIGN KEY ({nameof(AttendanceCompletion.SignatureId)}) REFERENCES Signatures({nameof(Signature.Id)})
            );

            CREATE TABLE IF NOT EXISTS EventAttendances (
                {nameof(Attendance.UserId)} TEXT,
                {nameof(Attendance.EventId)} INTEGER,
                {nameof(Attendance.CompletionId)} TEXT,
                {nameof(Attendance.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Attendance.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(Attendance.UserId)}, {nameof(Attendance.EventId)}),
                FOREIGN KEY ({nameof(Attendance.UserId)}) REFERENCES Users({nameof(User.Id)}),
                FOREIGN KEY ({nameof(Attendance.EventId)}) REFERENCES Events({nameof(OrganizationEvent.Id)}),
                FOREIGN KEY ({nameof(Attendance.CompletionId)}) REFERENCES AttendanceCompletions({nameof(AttendanceCompletion.Id)})
            );
            """);
    }
}
