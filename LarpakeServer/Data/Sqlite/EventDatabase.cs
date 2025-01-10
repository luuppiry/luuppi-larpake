using LarpakeServer.Helpers.Generic;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class EventDatabase(
    SqliteConnectionString connectionString, UserDatabase userDb)
    : SqliteDbBase(connectionString, userDb), IEventDatabase
{
    public async Task<OrganizationEvent[]> Get(EventQueryOptions options)
    {
        StringBuilder query = new();

        query.AppendLine($"""
            SELECT * FROM Events
            WHERE {nameof(OrganizationEvent.DeletedAt)} IS NULL
            """);

        // Add filters
        if (options.After is not null)
        {
            // Start time is after given time
            query.AppendLine($"""
                AND {nameof(OrganizationEvent.StartsAt)} >= @{nameof(options.After)} 
                """);
        }
        if (options.Before is not null)
        {
            // Start time is before given time
            query.AppendLine($"""
                AND {nameof(OrganizationEvent.StartsAt)} <= @{nameof(options.Before)} 
                """);
        }
        if (options.Title is not null)
        {
            // Title like given title (can include any characters around)
            query.AppendLine($"""
                AND {nameof(OrganizationEvent.Title)} LIKE %@{nameof(options.Title)}% 
                """);
        }

        query.Append($"""
            ORDER BY {nameof(OrganizationEvent.StartsAt)} ASC
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)};
            """);

        using var connection = await GetConnection();
        var events = await connection.QueryAsync<OrganizationEvent>(query.ToString(), options);
        return events.ToArray();
    }

    public async Task<OrganizationEvent?> Get(long id)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<OrganizationEvent>($"""
            SELECT * FROM Events WHERE {nameof(OrganizationEvent.Id)} = @id LIMIT 1;
            """, new { id });
    }

    public async Task<Result<long>> Insert(OrganizationEvent record)
    {
        try
        {
            var sql = $"""
                INSERT INTO Events (
                    {nameof(OrganizationEvent.Title)}, 
                    {nameof(OrganizationEvent.Body)}, 
                    {nameof(OrganizationEvent.StartsAt)},
                    {nameof(OrganizationEvent.EndsAt)}, 
                    {nameof(OrganizationEvent.Location)}, 
                    {nameof(OrganizationEvent.LuuppiRefId)},
                    {nameof(OrganizationEvent.WebsiteUrl)}, 
                    {nameof(OrganizationEvent.ImageUrl)}, 
                    {nameof(OrganizationEvent.CreatedBy)},
                    {nameof(OrganizationEvent.UpdatedBy)},
                    {nameof(OrganizationEvent.DeletedAt)}
                ) 
                Values (
                    @{nameof(OrganizationEvent.Title)},
                    @{nameof(OrganizationEvent.Body)},
                    @{nameof(OrganizationEvent.StartsAt)},
                    @{nameof(OrganizationEvent.EndsAt)},
                    @{nameof(OrganizationEvent.Location)},
                    @{nameof(OrganizationEvent.LuuppiRefId)},
                    @{nameof(OrganizationEvent.WebsiteUrl)},
                    @{nameof(OrganizationEvent.ImageUrl)},
                    @{nameof(OrganizationEvent.CreatedBy)},
                    @{nameof(OrganizationEvent.UpdatedBy)},
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

    public async Task<Result<int>> Update(OrganizationEvent record)
    {
        if (record.Id is Constants.NullId)
        {
            return new Error(400, $"Cannot update object with null id '{Constants.NullId}'.");
        }

        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE Events 
            SET 
                {nameof(OrganizationEvent.Title)} = @{nameof(record.Title)},
                {nameof(OrganizationEvent.Body)} = @{nameof(record.Body)},
                {nameof(OrganizationEvent.StartsAt)} = @{nameof(record.StartsAt)},
                {nameof(OrganizationEvent.EndsAt)} = @{nameof(record.EndsAt)},
                {nameof(OrganizationEvent.Location)} = @{nameof(record.Location)},
                {nameof(OrganizationEvent.LuuppiRefId)} = @{nameof(record.LuuppiRefId)},
                {nameof(OrganizationEvent.WebsiteUrl)} = @{nameof(record.WebsiteUrl)},
                {nameof(OrganizationEvent.ImageUrl)} = @{nameof(record.ImageUrl)},
                {nameof(OrganizationEvent.UpdatedBy)} = @{nameof(record.UpdatedBy)},
                {nameof(OrganizationEvent.UpdatedAt)} = DATETIME('now')
            WHERE 
                {nameof(OrganizationEvent.Id)} = @{nameof(record.Id)};
            """, record);
    }

    public async Task<int> Delete(long eventId, Guid modifyingUser)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE Events 
            SET 
                {nameof(OrganizationEvent.DeletedAt)} = DATETIME('now'),
                {nameof(OrganizationEvent.UpdatedAt)} = DATETIME('now'),
                {nameof(OrganizationEvent.UpdatedBy)} = @{nameof(modifyingUser)}
            WHERE 
                {nameof(OrganizationEvent.Id)} = @{nameof(eventId)};
            """, new { eventId, modifyingUser });
    }

    public async Task<int> HardDelete(long eventId)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM Events 
            WHERE 
                {nameof(OrganizationEvent.Id)} = @{nameof(eventId)};
            """, new { eventId });
    }

    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS Events (
                {nameof(OrganizationEvent.Id)} INTEGER,
                {nameof(OrganizationEvent.Title)} TEXT NOT NULL,
                {nameof(OrganizationEvent.Body)} TEXT,
                {nameof(OrganizationEvent.StartsAt)} DATETIME NOT NULL,
                {nameof(OrganizationEvent.EndsAt)} DATETIME DEFAULT NULL,
                {nameof(OrganizationEvent.Location)} TEXT NOT NULL,
                {nameof(OrganizationEvent.ImageUrl)} TEXT,
                {nameof(OrganizationEvent.LuuppiRefId)} INTEGER,
                {nameof(OrganizationEvent.WebsiteUrl)} TEXT,
                {nameof(OrganizationEvent.CreatedBy)} TEXT NOT NULL,
                {nameof(OrganizationEvent.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(OrganizationEvent.UpdatedBy)} TEXT NOT NULL,
                {nameof(OrganizationEvent.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(OrganizationEvent.DeletedAt)} DATETIME,
                FOREIGN KEY({nameof(OrganizationEvent.CreatedBy)}) REFERENCES Users({nameof(User.Id)}),
                FOREIGN KEY({nameof(OrganizationEvent.UpdatedBy)}) REFERENCES Users({nameof(User.Id)}),
                PRIMARY KEY({nameof(OrganizationEvent.Id)})
            )
            """);
    }

   
}
