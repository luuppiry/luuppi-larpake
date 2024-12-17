using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class EventDatabase(SqliteConnectionString connectionString, UserDatabase userDb)
    : SqliteDbBase(connectionString, userDb), IEventDatabase
{
    public async Task<Event[]> Get(EventQueryOptions options)
    {
        StringBuilder query = new();

        query.AppendLine($"""
            SELECT * FROM Events
            WHERE {nameof(Event.DeletedAt)} IS NULL
            """);

        // Add filters
        if (options.After is not null)
        {
            // Start time is after given time
            query.AppendLine($"""
                AND {nameof(Event.StartsAt)} >= @{nameof(options.After)} 
                """);
        }
        if (options.Before is not null)
        {
            // Start time is before given time
            query.AppendLine($"""
                AND {nameof(Event.StartsAt)} <= @{nameof(options.Before)} 
                """);
        }
        if (options.Title is not null)
        {
            // Title like given title (can include any characters around)
            query.AppendLine($"""
                AND {nameof(Event.Title)} LIKE %@{nameof(options.Title)}% 
                """);
        }

        query.Append($"""
            ORDER BY {nameof(Event.StartsAt)} ASC
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)};
            """);

        using var connection = await GetConnection();
        var events = await connection.QueryAsync<Event>(query.ToString(), options);
        return events.ToArray();
    }

    public async Task<Event?> Get(long id)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<Event>($"""
            SELECT * FROM Events WHERE {nameof(Event.Id)} = @id LIMIT 1;
            """, new { id });
    }

    public async Task<Result<long>> Insert(Event record)
    {
        try
        {
            var sql = $"""
                INSERT INTO Events (
                    {nameof(Event.Title)}, 
                    {nameof(Event.Body)}, 
                    {nameof(Event.StartsAt)},
                    {nameof(Event.EndsAt)}, 
                    {nameof(Event.Location)}, 
                    {nameof(Event.LuuppiRefId)},
                    {nameof(Event.WebsiteUrl)}, 
                    {nameof(Event.ImageUrl)}, 
                    {nameof(Event.CreatedBy)},
                    {nameof(Event.UpdatedBy)},
                    {nameof(Event.DeletedAt)}
                ) 
                Values (
                    @{nameof(Event.Title)},
                    @{nameof(Event.Body)},
                    @{nameof(Event.StartsAt)},
                    @{nameof(Event.EndsAt)},
                    @{nameof(Event.Location)},
                    @{nameof(Event.LuuppiRefId)},
                    @{nameof(Event.WebsiteUrl)},
                    @{nameof(Event.ImageUrl)},
                    @{nameof(Event.CreatedBy)},
                    @{nameof(Event.UpdatedBy)},
                    NULL
                );
                SELECT last_insert_rowid();
                """;
            using var connection = await GetConnection();
            return await connection.ExecuteScalarAsync<long>(sql, record);
        }
        catch (SqliteException ex)
        {
            switch (ex.SqliteExtendedErrorCode)
            {
                case SqliteError.PrimaryKey_e:
                    return new Error(500, "Could not create Id.");
                case SqliteError.ForeignKey_e:
                    return new Error(404, "Request user not found.");
                default:
                    throw;
            }
        }
    }

    public async Task<Result<int>> Update(Event record)
    {
        if (record.Id is Constants.NullId)
        {
            return new Error(400, $"Cannot update object with null id '{Constants.NullId}'.");
        }

        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE Events 
            SET 
                {nameof(Event.Title)} = @{nameof(record.Title)},
                {nameof(Event.Body)} = @{nameof(record.Body)},
                {nameof(Event.StartsAt)} = @{nameof(record.StartsAt)},
                {nameof(Event.EndsAt)} = @{nameof(record.EndsAt)},
                {nameof(Event.Location)} = @{nameof(record.Location)},
                {nameof(Event.LuuppiRefId)} = @{nameof(record.LuuppiRefId)},
                {nameof(Event.WebsiteUrl)} = @{nameof(record.WebsiteUrl)},
                {nameof(Event.ImageUrl)} = @{nameof(record.ImageUrl)},
                {nameof(Event.UpdatedBy)} = @{nameof(record.UpdatedBy)},
                {nameof(Event.UpdatedAt)} = DATETIME('now')
            WHERE 
                {nameof(Event.Id)} = @{nameof(record.Id)};
            """, record);
    }

    public async Task<int> Delete(long eventId, Guid modifyingUser)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE Events 
            SET 
                {nameof(Event.DeletedAt)} = DATETIME('now'),
                {nameof(Event.UpdatedAt)} = DATETIME('now'),
                {nameof(Event.UpdatedBy)} = @{nameof(modifyingUser)}
            WHERE 
                {nameof(Event.Id)} = @{nameof(eventId)};
            """, new { eventId, modifyingUser });
    }

    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS Events (
                {nameof(Event.Id)} INTEGER,
                {nameof(Event.Title)} TEXT NOT NULL,
                {nameof(Event.Body)} TEXT,
                {nameof(Event.StartsAt)} DATETIME NOT NULL,
                {nameof(Event.EndsAt)} DATETIME DEFAULT NULL,
                {nameof(Event.Location)} TEXT NOT NULL,
                {nameof(Event.ImageUrl)} TEXT,
                {nameof(Event.LuuppiRefId)} INTEGER,
                {nameof(Event.WebsiteUrl)} TEXT,
                {nameof(Event.CreatedBy)} TEXT NOT NULL,
                {nameof(Event.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Event.UpdatedBy)} TEXT NOT NULL,
                {nameof(Event.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Event.DeletedAt)} DATETIME,
                FOREIGN KEY({nameof(Event.CreatedBy)}) REFERENCES Users({nameof(User.Id)}),
                FOREIGN KEY({nameof(Event.UpdatedBy)}) REFERENCES Users({nameof(User.Id)}),
                PRIMARY KEY({nameof(Event.Id)})
            )
            """);
    }

}
