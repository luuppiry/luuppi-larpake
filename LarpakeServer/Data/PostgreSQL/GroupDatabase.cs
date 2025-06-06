﻿using LarpakeServer.Data.Helpers;
using LarpakeServer.Extensions;
using LarpakeServer.Models.Collections;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;
using Npgsql;
using System.Diagnostics;

namespace LarpakeServer.Data.PostgreSQL;

public class GroupDatabase : PostgresDb, IGroupDatabase
{
    readonly InviteKeyService _keyService;

    record struct InsertModel(long Id, Guid UserId);
    record struct InsertModel2(long Id, Guid UserId, bool IsHidden);


    public GroupDatabase(
        NpgsqlConnectionString connectionString,
        ILogger<GroupDatabase> logger,
        InviteKeyService keyService) : base(connectionString, logger)
    {
        _keyService = keyService;
    }


    public async Task<FreshmanGroup[]> GetGroups(FreshmanGroupQueryOptions options)
    {
        options.DoMinimize = false;

        SelectQuery query = new();
        query.AppendLine($"""
            SELECT DISTINCT ON (g.id)
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
            ORDER BY g.id, g.larpake_id, g.group_number ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);

        string parsed = query.ToString();
        using var connection = GetConnection();
        var rawGroups = await connection.QueryAsync<FreshmanGroup>(parsed, options);
        
        var groups = rawGroups.ToArray();
        var rawMembers = await connection.QueryAsync<FreshmanGroupMember>($"""
            SELECT 
                m.group_id,
                m.user_id,
                m.is_hidden,
                m.is_competing,
                m.joined_at
            FROM freshman_group_members m
            WHERE group_id = ANY(@Ids);
            """, new { Ids = groups.Select(x => x.Id).ToArray() });

        // Map members to groups
        Dictionary<long, List<FreshmanGroupMember>> memberMap = rawMembers
            .GroupBy(x => x.GroupId)
            .ToDictionary(x => x.Key, x => x.ToList());

        foreach (var group in groups)
        {
            if (memberMap.TryGetValue(group.Id, out var value))
            {
                group.Members = value;
            }
        }
        return groups;
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

        string parsed = query.ToString();
        using var connection = GetConnection();
        var minimized = await connection.QueryAsync<FreshmanGroup>(parsed, options);
        return minimized.ToArray();
    }

    private static void AddWhereClauses(ref SelectQuery query, in FreshmanGroupQueryOptions options)
    {
        SelectQuery.ConditionOperand operand = options.IsORQuery ?
            SelectQuery.ConditionOperand.Or : SelectQuery.ConditionOperand.And;

        // Are hidden members included?
        query.IfFalse(options.IncludeHiddenMembers).AppendConditionLine($"""
            m.is_hidden IS FALSE
            """);

        // Search only groups user is in
        query.IfNotNull(options.ContainsUser).AppendConditionLine($"""
            m.user_id = @{nameof(options.ContainsUser)}
            """, operand);

        // Match groups with name including case-insensitive string
        query.IfNotNull(options.GroupNameSearchValue).AppendConditionLine($"""
            g.name ILIKE @{nameof(options.GroupNameSearchValue)}
            """, operand);

        // Match groups with specific start year
        query.IfNotNull(options.StartYear).AppendConditionLine($"""
            l.year = @{nameof(options.StartYear)}
            """, operand);

        // Is participating in larpake
        query.IfNotNull(options.LarpakeId).AppendConditionLine($"""
            g.larpake_id = @{nameof(options.LarpakeId)}
            """, operand);

        // Group number
        query.IfNotNull(options.GroupNumber).AppendConditionLine($"""
            g.group_number = @{nameof(options.GroupNumber)}
            """, operand);

        // User search
        query.IfNotNull(options.ContainsUser)
          .IfNotNull(options.IsSearchMemberCompeting)
          .AppendConditionLine($"""
                m.is_competing = @{nameof(options.IsSearchMemberCompeting)}
                """, operand);


    }

    public async Task<FreshmanGroup?> GetGroup(long id)
    {
        using var connection = GetConnection();

        FreshmanGroup? result = null;
        await connection.QueryAsync<FreshmanGroup, FreshmanGroupMember, FreshmanGroup>($"""
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
                m.is_competing,
                m.joined_at
            FROM freshman_groups g
            LEFT JOIN freshman_group_members m
                ON g.id = m.group_id
            WHERE g.id = @{nameof(id)};
            """,
            (group, member) =>
            {
                result ??= group;
                if (member is not null)
                {
                    result.Members ??= [];
                    result.Members.Add(member);
                }
                return group;
            },
            new { id },
            splitOn: "group_id");

        return result;
    }

    public async Task<Guid[]?> GetMemberIds(long id)
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
        catch (NpgsqlException ex ) when (ex.SqlState is PostgresError.ForeignKeyViolation)
        {
            return Error.NotFound("Larpake not found.", ErrorCode.IdNotFound);
        }
        catch (NpgsqlException ex)
        {
            Logger.LogError(ex, "Unhandled exception thrown duirng freshman group insertion.");
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
                    @{nameof(InsertModel.UserId)}
                )
                ON CONFLICT DO NOTHING;
                """,
                records);
        }
        catch (NpgsqlException ex) when (ex.SqlState is PostgresError.UniqueViolation)
        {
            return Error.NotFound("User or group not found", ErrorCode.IdNotFound);
        }
        catch (NpgsqlException ex)
        {
            Logger.LogError(ex, "Unhandled exception during group member insertion");
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
                @{nameof(InsertModel2.IsHidden)},
                FALSE
            )
            ON CONFLICT (group_id, user_id) DO UPDATE
                SET 
                    is_competing = FALSE,
                    is_hidden = @{nameof(InsertModel2.IsHidden)};
            """,
                records);
        }
        catch (NpgsqlException ex) when (ex.SqlState is PostgresError.UniqueViolation)
        {
            return Error.NotFound("User or group not found", ErrorCode.IdNotFound);
        }
        catch (NpgsqlException ex)
        {
            Logger.LogError(ex, "Unhandled exception during non-competing group member insertion");
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
                """, record);
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
        Logger.LogTrace("Deleting members {ids} from group {id}", members, id);

        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM freshman_group_members
            WHERE group_id = @{nameof(id)}
                AND user_id IN (@x);
            """,
            members.Select(x => new { id, x }));
    }

    public async Task<Result<string>> GetInviteKey(long groupId)
    {
        // Get group invite key or create new if null
        string newKey = _keyService.GenerateKey();
        using var connection = GetConnection();

        // Get or update if null
        string? key = await connection.ExecuteScalarAsync<string>($"""
            UPDATE freshman_groups 
            SET 
                invite_key = COALESCE(invite_key, @{nameof(newKey)}),
                updated_at = CASE 
                    WHEN invite_key IS NULL THEN NOW() 
                    ELSE updated_at END,
                invite_key_changed_at = CASE 
                    WHEN invite_key IS NULL THEN NOW() 
                    ELSE invite_key_changed_at END
            WHERE id = @{nameof(groupId)}
            RETURNING invite_key;
            """, new { newKey, groupId });

        if (key is null)
        {
            Logger.LogTrace("Failed to retrieve invite key for group {id}", groupId);
            return Error.NotFound("Group not found.");
        }

        Logger.LogTrace("Retrieved invite key {key} for group {id}", key, groupId);
        return key;
    }

    public async Task<Result<int>> InsertMemberByInviteKey(string inviteKey, Guid userId)
    {
        if (inviteKey is null)
        {
            return Error.BadRequest("Invite key is null.");
        }
        if (inviteKey.Length != _keyService.KeyLength)
        {
            return Error.BadRequest("Invalid invite key length.");
        }

        Logger.LogTrace("Adding user {id} to group with invite key {key}", userId, inviteKey);

        try
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync($"""
            INSERT INTO freshman_group_members (
                group_id,
                user_id,
                is_competing
            )
            SELECT 
                id,
                @{nameof(userId)},
                TRUE
            FROM freshman_groups
            WHERE invite_key = @{nameof(inviteKey)}
            ON CONFLICT DO NOTHING;
            """, new { inviteKey, userId });
        }
        catch (NpgsqlException ex) when (ex.SqlState is PostgresError.ForeignKeyViolation)
        {
            return Error.NotFound("User or invite key not found.", ErrorCode.IdNotFound);
        }
        catch (NpgsqlException ex)
        {
            Logger.LogError(ex, "Error inserting member by invite key.");
            throw;
        }
    }

    public async Task<GroupInfo?> GetGroupByInviteKey(string inviteKey)
    {
        if (inviteKey is null)
        {
            return null;
        }
        if (inviteKey.Length != _keyService.KeyLength)
        {
            return null;
        }

        Logger.LogTrace("Retrieving information for group with invite key {key}", inviteKey);

        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<GroupInfo>($"""
            SELECT 
                larpake_id,
                name,
                group_number
            FROM freshman_groups
                WHERE invite_key = @{nameof(inviteKey)}
            LIMIT 1;
            """, new { inviteKey });
    }

    public async Task<Result<string>> RefreshInviteKey(long groupId)
    {
        // Get group invite key or create new if null
        string newKey = _keyService.GenerateKey();
        using var connection = GetConnection();

        Logger.LogTrace("Overwrite group {id} invite key with new value {key}", groupId, newKey);

        // Get or update if null
        string? key = await connection.ExecuteScalarAsync<string>($"""
            UPDATE freshman_groups 
            SET 
                invite_key = @{nameof(newKey)},
                updated_at = NOW(),
                invite_key_changed_at = NOW()
            WHERE id = @{nameof(groupId)}
            RETURNING invite_key;
            """, new { newKey, groupId });

        if (key is null)
        {
            return Error.NotFound("Group not found.");
        }
        return key;
    }



    record struct Member(long GroupId, Guid UserId, bool IsHidden, bool IsCompeting);

    public async Task<RawGroupMemberCollection[]> GetMembers(long[] groupIds, Guid userId, bool includeHidden)
    {
        using var connection = GetConnection();

        SelectQuery query = new();
        query.AppendLine($"""
            SELECT 
                g.id AS group_id,
                m.user_id,
                m.is_hidden,
                m.is_competing
            FROM freshman_groups g
                LEFT JOIN freshman_group_members m
                    ON g.id = m.group_id
            """);

        query.AppendConditionLine($"""
            g.id = ANY(
                SELECT group_id FROM freshman_group_members
                    WHERE group_id = ANY(@{nameof(groupIds)})
                        AND user_id = @{nameof(userId)}
            )
            """);

        query.IfFalse(includeHidden).AppendConditionLine($"""
            m.is_hidden = FALSE
            """);



        string parsed = query.ToString();
        var records = await connection.QueryAsync<Member>(parsed, new { groupIds, userId });

        Dictionary<long, RawGroupMemberCollection> memberMap = [];
        foreach (var user in records)
        {
            RawGroupMemberCollection? container = memberMap.GetOrAdd(user.GroupId, new(user.GroupId))
                ?? throw new UnreachableException("Container should never be null.");

            if (user.UserId == Guid.Empty)
            {
                continue;
            }
            if (user.IsCompeting)
            {
                container.Members.Add(user.UserId);
            }
            else
            {
                container.Tutors.Add(user.UserId);
            }
        }

        return memberMap.Values.ToArray();
    }
}
