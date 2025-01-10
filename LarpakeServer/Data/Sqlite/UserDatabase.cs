using LarpakeServer.Helpers.Generic;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class UserDatabase(SqliteConnectionString connectionString)
    : SqliteDbBase(connectionString), IUserDatabase
{
    public async Task<User[]> Get(UserQueryOptions options)
    {
        SelectQuery query = new();
        query.AppendLine($"""
            SELECT 
                {nameof(User.Id)},
                {nameof(User.Permissions)},
                {nameof(User.StartYear)},
                {nameof(User.CreatedAt)},
                {nameof(User.UpdatedAt)}
            FROM Users
            """);

        if (options.Permissions is not null)
        {
            query.AppendConditionLine($"""
                {nameof(User.Permissions)} & @{nameof(options.Permissions)} = @{nameof(options.Permissions)}
                """);
        }
        if (options.StartedOnOrAfter is not null)
        {
            query.AppendConditionLine($"""
                {nameof(User.StartYear)} >= @{nameof(options.StartedOnOrAfter)}
                """);
        }
        if (options.StartedOnOrBefore is not null)
        {
            query.AppendConditionLine($"""
                {nameof(User.StartYear)} <= @{nameof(options.StartedOnOrBefore)}
                """);
        }

        query.AppendLine($"""
            ORDER BY {nameof(User.StartYear)}, {nameof(User.Id)} ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);

        using var connection = await GetConnection();
        var records = await connection.QueryAsync<User>(query.Build(), options);
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
        catch (SqliteException ex) when (ex.SqliteErrorCode is SqliteError.Constraint)
        {
            switch (ex.SqliteExtendedErrorCode)
            {
                case SqliteError.PrimaryKey_e:
                    return Error.InternalServerError("Failed to create unique id.");
                default:
                    throw;
            }
        }
    }

    public async Task<Result<int>> Update(User record)
    {
        if (record.Id == Guid.Empty)
        {
            return Error.BadRequest("Id is required.");
        }

        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE Users 
            SET
                {nameof(User.Permissions)} = @{nameof(User.Permissions)},
                {nameof(User.StartYear)} = @{nameof(User.StartYear)},
                {nameof(User.UpdatedAt)} = DATETIME('now')
            WHERE {nameof(User.Id)} = @{nameof(User.Id)};
            """, record);
    }

    public async Task<Result<int>> UpdatePermissions(Guid id, Permissions permissions)
    {
        if (id == Guid.Empty)
        {
            return Error.BadRequest("Id is required.");
        }

        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE Users 
            SET
                {nameof(User.Permissions)} = @{nameof(permissions)},
                {nameof(User.UpdatedAt)} = DATETIME('now')
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
                {nameof(User.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(User.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(User.Id)})
            );
            """);
    }
}
