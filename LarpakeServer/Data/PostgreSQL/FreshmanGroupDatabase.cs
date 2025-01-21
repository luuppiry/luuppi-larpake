using LarpakeServer.Data.Helpers;
using LarpakeServer.Extensions;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class FreshmanGroupDatabase(NpgsqlConnectionString connectionString, ILogger<FreshmanGroupDatabase> logger)
    : PostgresDb(connectionString, logger), IFreshmanGroupDatabase
{
    record struct InsertModel(long Id, Guid UserId);


    public async Task<FreshmanGroup[]> GetGroups(FreshmanGroupQueryOptions options)
    {
        bool searchesUser = options.ContainsUser is not null;
        bool joinsRequired = searchesUser || options.DoMinimize is false;

        SelectQuery query = new();
        query.AppendLine($"""
            SELECT * FROM freshman_groups g
            """);

        // Join tables if necessary
        query.If(joinsRequired).AppendLine($"""
            LEFT JOIN freshman_group_members m
                ON g.id = m.group_id
            LEFT JOIN users u
                ON u.id = m.user_id
            """);

        // Search only groups user is in
        query.If(searchesUser).AppendConditionLine($"""
            u.id = @{nameof(options.ContainsUser)}
            """);

        // Match groups with name including case-insensitive string
        query.IfNotNull(options.GroupName).AppendConditionLine($"""
            g.name ILIKE %@{nameof(options.GroupName)}%
            """);

        // Match groups with specific start year
        query.IfNotNull(options.StartYear).AppendConditionLine($"""
            g.start_year = @{nameof(options.StartYear)}
            """);

        // Are hidden members included?
        query.If(options.IncludeHiddenMembers is false).AppendConditionLine($"""
            m.is_hidden = FALSE
            """);

        query.AppendLine($"""
            ORDER BY g.start_year, g.group_number ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);


        using var connection = GetConnection();
        if (options.DoMinimize)
        {
            var minimized = await connection.QueryAsync<FreshmanGroup>(query.ToString(), options);
            return minimized.ToArray();
        }


        Dictionary<long, FreshmanGroup> groups = [];
        await connection.QueryAsync<FreshmanGroup, FreshmanGroupMember, FreshmanGroup>(
            query.ToString(),
            (group, member) =>
            {
                FreshmanGroup resultGroup = groups.GetOrAdd(group.Id, group)!;
                resultGroup.Members ??= [];
                resultGroup.Members.Add(member.UserId);
                return resultGroup;
            },
            options,
            splitOn: "id");

        return groups.Values.ToArray();
    }

    public async Task<FreshmanGroup?> GetGroup(long id)
    {
        using var connection = GetConnection();

        FreshmanGroup? result = null;
        await connection.QueryAsync<FreshmanGroup, FreshmanGroupMember, FreshmanGroup>($"""
            SELECT * FROM freshman_groups g
            LEFT JOIN freshman_group_members m
                ON g.id = m.group_id
            WHERE g.id = @{nameof(id)};
            """,
            (group, member) =>
            {
                result ??= group;
                result.Members ??= [];
                result.Members.Add(member.UserId);
                return group;
            },
            new { id },
            splitOn: "id");

        return result;
    }

    public async Task<Guid[]?> GetMembers(long id)
    {
        using var connection = GetConnection();
        var members = await connection.QueryAsync<Guid>($"""
            SELECT user_id FROM freshman_group_members
            WHERE group_id = @{nameof(id)}
            """,
            new { id });

        return members.ToArray();
    }

    public async Task<Result<long>> Insert(FreshmanGroup record)
    {
        try
        {
            using var connection = GetConnection();
            return await connection.ExecuteScalarAsync<long>($"""
                INSERT INTO freshman_groups (
                    name, 
                    start_year, 
                    group_number
                )
                VALUES (
                    @{nameof(record.Name)},
                    @{nameof(record.StartYear)},
                    @{nameof(record.GroupNumber)}
                )
                RETURNING id;
                """);
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError("Error inserting freshman group: {msg}", ex.Message);
            throw;
        }
    }

    public Task<Result<int>> InsertHiddenMembers(long groupId, Guid[] members)
    {
        return InsertMembers(groupId, members, isHidden: true);
    }

    public Task<Result<int>> InsertMembers(long groupId, Guid[] members)
    {
        return InsertMembers(groupId, members, isHidden: false);
    }

    public async Task<Result<int>> Update(FreshmanGroup record)
    {
        try
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync($"""
                UPDATE freshman_groups
                SET
                    name = @{nameof(record.Name)},
                    start_year = @{nameof(record.StartYear)},
                    group_number = @{nameof(record.GroupNumber)},
                    updated_at = NOW()
                WHERE id = @{nameof(record.Id)}
                """);
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError("Error updating freshman group: {msg}", ex.Message);
            throw;
        }
    }

    public async Task<int> Delete(long id)
    {
        using var connection = GetConnection();

        int rowsAffected = connection.Execute($"""
            DELETE FROM freshman_groups WHERE id = @{nameof(id)}
            """,
            new { id });

        rowsAffected += await connection.ExecuteAsync($"""
            DELETE FROM freshman_group_members WHERE group_id = @{nameof(id)}
            """,
            new { id });

        Logger.IfPositive(rowsAffected).LogInformation("Deleted group with id {id}.", id);
        return rowsAffected;
    }

    public async Task<int> DeleteMembers(long id, Guid[] members)
    {
        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM freshman_group_members
            WHERE group_id = @{nameof(id)}
                AND user_id IN (@{nameof(members)});
            """,
            new { id, members });
    }


    private async Task<Result<int>> InsertMembers(long groupId, Guid[] members, bool isHidden)
    {
        var records = members
            .Distinct()
            .Select(x => new InsertModel(groupId, x));

        using var connection = GetConnection();
        try
        {
            return await connection.ExecuteAsync($"""
                INSERT INTO freshman_group_members (
                    group_id,
                    user_id,
                    is_hidden
                )
                VALUES (
                    @{nameof(InsertModel.Id)},
                    @{nameof(InsertModel.UserId)},
                    {(isHidden ? "TRUE" : "FALSE")}
                )
                ON CONFLICT DO NOTHING;
                """,
                records);
        }
        catch (NpgsqlException e)
        {
            // TODO: Handle error "when (e.SqlState == "23505")"
            Logger.LogError("Failed to insert hidden members: {ex}", e.Message);
            throw;
        }
    }
}
