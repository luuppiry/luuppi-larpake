﻿using LarpakeServer.Data.Helpers;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class UserDatabase(NpgsqlConnectionString connectionString)
    : PostgresDb(connectionString), IUserDatabase
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
                updated_at,
                entra_id,
                entra_username
            FROM users
            """);

        SelectQuery.ConditionOperand operand = options.IsORQuery ? SelectQuery.ConditionOperand.Or : SelectQuery.ConditionOperand.And;


        query.IfNotNull(options.EntraIds).AppendConditionLine($"""
            entra_id = ANY(@{nameof(options.EntraIds)})
            """, operand);

        query.IfNotNull(options.UserIds).AppendConditionLine($"""
            id = ANY(@{nameof(options.UserIds)})
            """, operand);

        query.IfNotNull(options.EntraUsername).AppendConditionLine($"""
            entra_username ILIKE @{nameof(options.EntraUsernameQueryValue)}
            """, operand);

        // If has at least permissions
        query.IfNotNull(options.Permissions).AppendConditionLine($"""
            permissions & @{nameof(options.Permissions)} = @{nameof(options.Permissions)}
            """, operand);

        // If start year after
        query.IfNotNull(options.StartedAfter).AppendConditionLine($"""
            start_year > @{nameof(options.StartedAfter)}
            """, operand);

        // If start year before
        query.IfNotNull(options.StartedBefore).AppendConditionLine($"""
            start_year < @{nameof(options.StartedBefore)}
            """, operand);

        query.AppendLine($"""
            ORDER BY start_year, id ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);

        string parsed = query.ToString();
        using var connection = GetConnection();
        var users = await connection.QueryAsync<User>(parsed, options);
        return users.ToArray();
    }

    public async Task<User?> GetByUserId(Guid id)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<User>($"""
            SELECT 
                id,
                permissions,
                start_year,
                created_at,
                updated_at,
                entra_id,
                entra_username
            FROM users 
            WHERE id = @{nameof(id)} LIMIT 1;
            """, new { id });

    }

    public async Task<User?> GetByEntraId(Guid entraId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<User>($"""
            SELECT 
                id,
                permissions,
                start_year,
                created_at,
                updated_at,
                entra_id,
                entra_username
            FROM users 
            WHERE entra_id = @{nameof(entraId)} LIMIT 1;
            """, new { entraId });
    }

    public async Task<Result<Guid>> Insert(User record)
    {
        /* UUID_v7 conflict is possible here,
         * but is not worth handling, as it is very unlikely to happen. 
         */

        record.Id = Guid.CreateVersion7();
        using var connection = GetConnection();

        try
        {
            await connection.ExecuteAsync($"""
                INSERT INTO users (
                    id,
                    start_year,
                    entra_id,
                    entra_username
                )
                VALUES (
                    @{nameof(User.Id)},
                    @{nameof(User.StartYear)},
                    @{nameof(User.EntraId)},
                    @{nameof(User.EntraUsername)}
                );
                """, record);
        }
        catch (NpgsqlException ex) when (ex.SqlState == PostgresError.UniqueViolation)
        {
            return Error.InternalServerError("Failed to generate unique id, retry request", ErrorCode.KeyGenFailed);
        }
        return record.Id;
    }

    public async Task<Result<int>> Update(User record)
    {
        if (record.Id == Guid.Empty)
        {
            return Error.BadRequest("Id is required.", ErrorCode.NullId);
        }

        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE users
            SET
                start_year = @{nameof(User.StartYear)},
                updated_at = NOW()
            WHERE id = @{nameof(User.Id)};
            """, record);
    }

    public async Task<Result<int>> SetPermissions(Guid id, Permissions permissions)
    {
        return await SetPermissions(id, permissions, isAppUserId: true);
    }

    public async Task<Result<int>> SetPermissionsByEntra(Guid id, Permissions permissions)
    {
        return await SetPermissions(id, permissions, isAppUserId: false);
    }

    public async Task<Result<int>> AppendPermissions(Guid id, Permissions permissions)
    {
        return await AppendPermissions(id, permissions, isAppUserId: true);
    }

    public async Task<Result<int>> AppendPermissionsByEntra(Guid id, Permissions permissions)
    {
        return await AppendPermissions(id, permissions, isAppUserId: false);
    }

    public async Task<int> Delete(Guid id)
    {
        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM users WHERE id = @{nameof(id)};
            """, new { id });
    }


    private async Task<Result<int>> AppendPermissions(Guid id, Permissions permissions, bool isAppUserId)
    {
        if (id == Guid.Empty)
        {
            return Error.BadRequest("Id is required.", ErrorCode.NullId);
        }

        string idField = isAppUserId ? "id" : "entra_id";

        using var connection = GetConnection();
        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE users
            SET
                permissions = permissions | @{nameof(permissions)},
                updated_at = NOW()
            WHERE {idField} = @{nameof(id)};
            """, new { id, permissions });

        string idType = isAppUserId ? "user" : "entra user";
        Logger.IfPositive(rowsAffected)
            .LogTrace("Appended permissions {permissions} to {type} {id}.", permissions, idType, id);

        Logger.IfZero(rowsAffected)
            .LogInformation("Cannot append permissions, {type} {id} not found.", idType, id);

        return rowsAffected;
    }
    private async Task<Result<int>> SetPermissions(Guid id, Permissions permissions, bool isAppUserId)
    {
        if (id == Guid.Empty)
        {
            return Error.BadRequest("Id is required.", ErrorCode.NullId);
        }

        string idField = isAppUserId ? "id" : "entra_id";

        using var connection = GetConnection();
        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE users
            SET
                permissions = @{nameof(permissions)},
                updated_at = NOW()
            WHERE {idField} = @{nameof(id)};
            """, new { id, permissions });

        Logger.IfPositive(rowsAffected)
            .LogTrace("Set permissions {permissions} to entra user {id}.", permissions, id);

        Logger.IfZero(rowsAffected)
            .LogInformation("Cannot set permissions, entra user {id} not found.", id);

        return rowsAffected;
    }
}
