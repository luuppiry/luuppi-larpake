using LarpakeServer.Data.Helpers;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data.PostgreSQL;

public class UserDatabase(NpgsqlConnectionString connectionString) : PostgresDb(connectionString), IUserDatabase
{
    public async Task<User[]> Get(UserQueryOptions options)
    {
        SelectQuery query = new();

        query.AppendLine($"""
            SELECT 
                id,
                permissions,
                start_year,
                created_at,
                updated_at
            FROM users
            """);

        // If has at least permissions
        query.IfNotNull(options.Permissions).AppendConditionLine($"""
            permissions & @{nameof(options.Permissions)} = @{nameof(options.Permissions)}
            """);
        
        // If start year after
        query.IfNotNull(options.StartedAfter).AppendConditionLine($"""
            start_year > @{nameof(options.StartedAfter)}
            """);

        // If start year before
        query.IfNotNull(options.StartedBefore).AppendConditionLine($"""
            start_year < @{nameof(options.StartedBefore)}
            """);

        query.AppendLine($"""
            ORDER BY start_year, id ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);

        using var connection = GetConnection();
        var users = await connection.QueryAsync<User>(query.Build(), options);
        return users.ToArray();
    }

    public async Task<User?> Get(Guid id)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<User>($"""
            SELECT 
                id,
                permissions,
                start_year,
                created_at,
                updated_at
            FROM users 
            WHERE id = @{nameof(id)} LIMIT 1;
            """, new { id });

    }

    public async Task<Result<Guid>> Insert(User record)
    {
        /* UUID_v7 conflict is possible here,
         * but is not worth handling, as it is very unlikely to happen. 
         */

        record.Id = Guid.CreateVersion7();
        using var connection = GetConnection();

        await connection.ExecuteAsync($"""
            INSERT INTO users (
                id,
                start_year
            )
            VALUES (
                @{nameof(User.Id)},
                @{nameof(User.StartYear)}
            );
            """, record);

        return record.Id;
    }

    public async Task<Result<int>> Update(User record)
    {
        if (record.Id == Guid.Empty)
        {
            return Error.BadRequest("Id is required.");
        }

        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE users
            SET
                permissions = @{nameof(User.Permissions)},
                start_year = @{nameof(User.StartYear)},
                updated_at = NOW()
            WHERE id = @{nameof(User.Id)};
            """, record);
    }

    public async Task<Result<int>> SetPermissions(Guid id, Permissions permissions)
    {
        if (id == Guid.Empty)
        {
            return Error.BadRequest("Id is required.");
        }

        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE users
            SET
                permissions = @{nameof(permissions)},
                updated_at = NOW()
            WHERE id = @{nameof(id)};
            """, new { id, permissions });
    }

    public Task<int> Delete(Guid id)
    {
        using var connection = GetConnection();
        return connection.ExecuteAsync($"""
            DELETE FROM users WHERE id = @{nameof(id)};
            """, new { id });
    }
}
