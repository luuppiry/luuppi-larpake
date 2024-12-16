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
        StringBuilder query = new();
        query.AppendLine($"""
            SELECT * FROM EventAttendances a
            LEFT JOIN AttendanceCompletions c
                ON a.{nameof(Attendance.CompletionId)} = c.{nameof(AttendanceCompletion.Id)}
            WHERE TRUE
            """);

        if (options.UserId is not null)
        {
            query.AppendLine($"""
                AND {nameof(Attendance.UserId)} = @{nameof(options.UserId)}
                """);
        }
        if (options.EventId is not null)
        {
            query.AppendLine($"""
                AND {nameof(Attendance.EventId)} = @{nameof(options.EventId)}
                """);
        }
        if (options.IsCompleted is true)
        {
            query.AppendLine($"""
                AND {nameof(Attendance.CompletionId)} IS NOT NULL
                """);
        }
        if (options.IsCompleted is false)
        {
            query.AppendLine($"""
                AND {nameof(Attendance.CompletionId)} IS NULL
                """);
        }
        if (options.AfterUtc is not null)
        {
            query.AppendLine($"""
                AND {nameof(Attendance.CreatedUtc)} >= @{nameof(options.AfterUtc)}
                """);
        }
        if (options.BeforeUtc is not null)
        {
            query.AppendLine($"""
                AND {nameof(Attendance.CreatedUtc)} <= @{nameof(options.BeforeUtc)}
                """);
        }
        query.Append($"""
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
                INSERT INTO Attendances (
                    {nameof(Attendance.UserId)}, 
                    {nameof(Attendance.EventId)}, 
                    {nameof(Attendance.CompletionId)}
                )
                VALUES (
                    @{nameof(attendance.UserId)},
                    @{nameof(attendance.EventId)},
                    NULL
                );
                """);
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
            using var connection = await GetConnection();
            return await connection.ExecuteAsync($"""
                DELETE FROM AttendanceCompletions
                    WHERE {nameof(AttendanceCompletion.Id)} IN (
                        SELECT {nameof(Attendance.CompletionId)} FROM EventAttendances
                        WHERE {nameof(Attendance.UserId)} = @{nameof(userId)}
                            AND {nameof(Attendance.EventId)} = @{nameof(eventId)}
                );

                UPDATE Attendances SET
                    {nameof(Attendance.UserId)} = @{nameof(userId)}, 
                    {nameof(Attendance.EventId)} = @{nameof(eventId)}, 
                    {nameof(Attendance.CompletionId)} = NULL,
                    {nameof(Attendance.LastModified)} = DATETIME('now')
                );
                """, new { userId, eventId });
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

        try
        {
            completion.Id = Guid.CreateVersion7();

            using var connection = await GetConnection();

            // This query inserts AttendanceCompletion only id event
            // attendance with userId and eventId exists
            await connection.QueryFirstOrDefaultAsync($"""
                INSERT INTO AttendanceCompletions (
                    {nameof(AttendanceCompletion.Id)}, 
                    {nameof(AttendanceCompletion.SignerId)}, 
                    {nameof(AttendanceCompletion.SignatureId)},
                    {nameof(AttendanceCompletion.CompletionTimeUtc)}
                )
                SELECT
                    @{nameof(completion.Id)},
                    @{nameof(completion.SignerId)},
                    @{nameof(completion.SignatureId)},
                    @{nameof(completion.CompletionTimeUtc)},
                WHERE EXISTS (
                    SELECT 1 FROM EventAttendances
                    WHERE {nameof(Attendance.UserId)} = @{nameof(completion.UserId)}
                        AND {nameof(Attendance.EventId)} = @{nameof(completion.EventId)}
                    LIMIT 1
                );

                UPDATE EventAttendances 
                SET
                    {nameof(Attendance.CompletionId)} = @{nameof(completion.Id)}, 
                    {nameof(Attendance.LastModified)} = DATETIME('now')
                )
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
                {nameof(AttendanceCompletion.Id)} INTEGER,
                {nameof(AttendanceCompletion.SignerId)} NOT NULL,
                {nameof(AttendanceCompletion.SignatureId)} TEXT,
                {nameof(AttendanceCompletion.CompletionTimeUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(AttendanceCompletion.CreatedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(AttendanceCompletion.LastModifiedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(AttendanceCompletion.Id)}),
                FOREIGN KEY ({nameof(AttendanceCompletion.SignerId)}) REFERENCES Users({nameof(User.Id)}),
                FOREIGN KEY ({nameof(AttendanceCompletion.SignatureId)}) REFERENCES Signatures({nameof(Signature.Id)})
            );

            CREATE TABLE IF NOT EXISTS EventAttendances (
                {nameof(Attendance.UserId)} TEXT,
                {nameof(Attendance.EventId)} INTEGER,
                {nameof(Attendance.CompletionId)} INTEGER,
                {nameof(Attendance.CreatedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Attendance.LastModified)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(Attendance.UserId)}, {nameof(Attendance.EventId)}),
                FOREIGN KEY ({nameof(Attendance.UserId)}) REFERENCES Users({nameof(User.Id)}),
                FOREIGN KEY ({nameof(Attendance.EventId)}) REFERENCES Events({nameof(Event.Id)}),
                FOREIGN KEY ({nameof(Attendance.CompletionId)}) REFERENCES Events({nameof(AttendanceCompletion.Id)})
            );
            """);
    }
}
