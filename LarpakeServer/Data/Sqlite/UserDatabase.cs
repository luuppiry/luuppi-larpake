using LarpakeServer.Models;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Data.Sqlite;
using System.Reflection.Metadata;
using System.Text;

namespace LarpakeServer.Data.Sqlite;

public class UserDatabase(SqliteConnectionString connectionString)
    : SqliteDbBase(connectionString), IUserDatabase
{


    public async Task<User[]> Get(UserQueryOptions options)
    {
        StringBuilder query = new();
        query.AppendLine($"""
            SELECT * FROM Users
            """);

        if (options.Permissions is not null)
        {
            /* TODO: Permissions are not as straightforward as this query
             * This works if main permission roles are only used */
            query.AppendLine($"""
                WHERE {nameof(User.Permissions)} >= @{nameof(options.Permissions)}
                """);
        }
        if (options.StartedOnOrAfter is not null)
        {
            query.AppendLine($"""
                WHERE {nameof(User.StartYear)} >= @{nameof(options.StartedOnOrAfter)}
                """);
        }
        if (options.StartedOnOrBefore is not null)
        {
            query.AppendLine($"""
                WHERE {nameof(User.StartYear)} <= @{nameof(options.StartedOnOrBefore)}
                """);
        }

        query.AppendLine($"""
            ORDER BY {nameof(User.StartYear)}, {nameof(User.Id)} ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);

        using var connection = await GetConnection();
        var records = await connection.QueryAsync<User>(query.ToString(), options);
        return records.ToArray();
    }

    public async Task<User?> Get(Guid id)
    {
        string query = $"""
            SELECT * FROM Users WHERE {nameof(User.Id)} = @{nameof(id)} LIMIT 1;
            """;

        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { id });
    }


    public async Task<Result<Guid>> Insert(User record)
    {
        /* UUIDv7 is used to generate a unique Id
         * This is a very rare case, but if a collision occurs,
         * the server will retry max of 5 times */

        return await InsertRetriedRecursive(record, 5);
    }

    public async Task<Result<int>> Update(User record)
    {
        if (record.Id == Guid.Empty)
        {
            return new Error(400, "Id is required.");
        }

        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE Users 
            SET
                {nameof(User.Permissions)} = @{nameof(User.Permissions)},
                {nameof(User.StartYear)} = @{nameof(User.StartYear)},
                {nameof(User.LastModifiedUtc)} = DATETIME('now')
            WHERE {nameof(User.Id)} = @{nameof(User.Id)};
            """, record);
    }

    public async Task<Result<int>> UpdatePermissions(Guid id, Permissions permissions)
    {
        if (id == Guid.Empty)
        {
            return new Error(400, "Id is required.");
        }

        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE Users 
            SET
                {nameof(User.Permissions)} = @{nameof(permissions)},
                {nameof(User.LastModifiedUtc)} = DATETIME('now')
            WHERE {nameof(User.Id)} = @{nameof(id)};
            """, new { id, permissions });
    }

    public async Task<int> Delete(Guid id)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM Users WHERE {nameof(User.Id)} = @{nameof(id)};
            """, new { id });
    }



    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS Users (
                {nameof(User.Id)} TEXT,
                {nameof(User.Permissions)} INTEGER NOT NULL DEFAULT 0,
                {nameof(User.StartYear)} INTEGER,
                {nameof(User.CreatedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(User.LastModifiedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(User.Id)})
            );
            """);
    }

    private async Task<Result<Guid>> InsertRetriedRecursive(User record, int retriesLeft = 0)
    {
        if (retriesLeft <= 0)
        {
            return new Error(500, "Server failed to generate a unique Id, try resending the request.");
        }
        record.Id = Guid.CreateVersion7();
        try
        {
            string query = $"""
                INSERT INTO Users (
                    {nameof(User.Id)}, 
                    {nameof(User.StartYear)})
                VALUES (
                    @{nameof(User.Id)}, 
                    @{nameof(User.StartYear)});
                """;

            using var connection = await GetConnection();
            await connection.ExecuteAsync(query, record);
            return record.Id;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 19)
        {
            switch (ex.SqliteExtendedErrorCode)
            {
                case SqliteError.PrimaryKey_e:
                    // Retry with new Id, UUIDv7 should happen very rarely.
                    return await InsertRetriedRecursive(record, retriesLeft--);
                default:
                    throw;
            }
        }
    }
}
