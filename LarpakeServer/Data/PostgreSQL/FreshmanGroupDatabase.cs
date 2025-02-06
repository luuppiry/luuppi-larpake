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
    record struct InsertModel2(long Id, Guid UserId, bool IsHidden);


    public async Task<FreshmanGroup[]> GetGroups(FreshmanGroupQueryOptions options)
    {
        options.DoMinimize = false;

        SelectQuery query = new();
        query.AppendLine($"""
            SELECT 
                g.id,
                g.larpake_id,
                g.name,
                g.group_number,
                g.created_at,
                g.updated_at,
                m.group_id,
                m.user_id,
                m.is_hidden,
                m.joined_at
            FROM freshman_groups g
                LEFT JOIN freshman_group_members m
                    ON g.id = m.group_id
                LEFT JOIN larpakkeet l
                    ON g.larpake_id = l.id
            """);

        AddWhereClauses(ref query, options);

        query.AppendLine($"""
            ORDER BY l.year ASC, g.larpake_id, g.group_number ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);

        string q = query.ToString();

        using var connection = GetConnection();
        Dictionary<long, FreshmanGroup> groups = [];
        await connection.QueryAsync<FreshmanGroup, FreshmanGroupMember, FreshmanGroup>(
            query.ToString(),
            (group, member) =>
            {
                FreshmanGroup resultGroup = groups.GetOrAdd(group.Id, group)!;
                if (member is not null)
                {
                    resultGroup.Members ??= [];
                    resultGroup.Members.Add(member.UserId);
                }
                return resultGroup;
            },
            options,
            splitOn: "group_id");

        return groups.Values.ToArray();
    }


    public async Task<FreshmanGroup[]> GetGroupsMinimized(FreshmanGroupQueryOptions options)
    {
        options.DoMinimize = true;

        SelectQuery query = new();
        query.AppendLine($"""
            SELECT 
                g.id,
                g.larpake_id,
                g.name,
                g.group_number,
                g.created_at,
                g.updated_at
            FROM freshman_groups g
                LEFT JOIN freshman_group_members m
                    ON g.id = m.group_id
                LEFT JOIN larpakkeet l
                    ON g.larpake_id = l.id
            """);

        AddWhereClauses(ref query, options);
        query.AppendLine($"""
            ORDER BY l.year ASC, g.larpake_id, g.group_number ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);
        
        string q = query.ToString();

        using var connection = GetConnection();
        var minimized = await connection.QueryAsync<FreshmanGroup>(query.ToString(), options);
        return minimized.ToArray();
    }

    private static void AddWhereClauses(ref SelectQuery query, in FreshmanGroupQueryOptions options)
    {
        // Search only groups user is in
        query.IfNotNull(options.ContainsUser).AppendConditionLine($"""
            m.user_id = @{nameof(options.ContainsUser)}
            """);

        // Match groups with name including case-insensitive string
        query.IfNotNull(options.GroupName).AppendConditionLine($"""
            g.name ILIKE %@{nameof(options.GroupName)}%
            """);

        // Match groups with specific start year
        query.IfNotNull(options.StartYear).AppendConditionLine($"""
            l.year = @{nameof(options.StartYear)}
            """);

        // Are hidden members included?
        query.IfFalse(options.IncludeHiddenMembers).AppendConditionLine($"""
            m.is_hidden IS NOT TRUE
            """);

        // Is participating in larpake
        query.IfNotNull(options.LarpakeId).AppendConditionLine($"""
            g.larpake_id = @{nameof(options.LarpakeId)}
            """);
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
                    larpake_id, 
                    group_number
                )
                VALUES (
                    @{nameof(record.Name)},
                    @{nameof(record.LarpakeId)},
                    @{nameof(record.GroupNumber)}
                )
                RETURNING id;
                """, record);
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError(ex, "Error inserting freshman group.");
            throw;
        }
    }


    public async Task<Result<int>> InsertMembers(long groupId, Guid[] members)
    {
        var records = members
            .Distinct()
            .Select(x => new InsertModel(groupId, x))
            .ToArray();

        using var connection = GetConnection();
        try
        {
            return await connection.ExecuteAsync($"""
                INSERT INTO freshman_group_members (
                    group_id,
                    user_id
                )
                VALUES (
                    @{nameof(InsertModel.Id)},
                    @{nameof(InsertModel.UserId)},
                )
                ON CONFLICT DO NOTHING;
                """,
                records);
        }
        catch (NpgsqlException ex)
        {
            // TODO: Handle error "when (e.SqlState == "23505")"
            Logger.LogError(ex, "Failed to insert members");
            throw;
        }
    }


    public async Task<Result<int>> InsertNonCompeting(long groupId, NonCompetingMember[] members)
    {
        var records = members
            .DistinctBy(x => x.UserId)
            .Select(x => new InsertModel2(groupId, x.UserId, x.IsHidden))
            .ToArray();

        try
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync($"""
            INSERT INTO freshman_group_members (
                group_id,
                user_id,
                is_hidden,
                is_competing
            )
            VALUES (
                @{nameof(InsertModel2.Id)},
                @{nameof(InsertModel2.UserId)},
                @{nameof(InsertModel2.IsHidden)}
                FALSE
            )
            ON CONFLICT DO NOTHING;
            """,
                records);
        }
        catch (NpgsqlException ex)
        {
            // TODO: Handle error "when (e.SqlState == "23505")"
            Logger.LogError(ex, "Failed to insert non competing members");
            throw;
        }
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
                    group_number = @{nameof(record.GroupNumber)},
                    updated_at = NOW()
                WHERE id = @{nameof(record.Id)}
                """);
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError(ex, "Error updating freshman group.");
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

   



  
}
