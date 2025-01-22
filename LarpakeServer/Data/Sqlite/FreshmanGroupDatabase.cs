using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Data.Sqlite;


namespace LarpakeServer.Data.Sqlite;

public class FreshmanGroupDatabase : SqliteDbBase, IFreshmanGroupDatabase
{
    public FreshmanGroupDatabase(
        SqliteConnectionString connectionString,
        UserDatabase userDb)
        : base(connectionString, userDb)
    {
    }

    public async Task<FreshmanGroup[]> GetGroups(FreshmanGroupQueryOptions options)
    {
        SelectQuery query = new();

        bool searchUser = options.ContainsUser is not null;
        bool doJoins = searchUser || options.DoMinimize is false;

        query.AppendLine($"""
                SELECT * FROM FreshmanGroups fg
                """);


        if (doJoins)
        {
            query.AppendLine($"""
                LEFT JOIN FreshmanGroupMembers fgm 
                    ON fg.{nameof(FreshmanGroup.Id)} = fgm.{FGM_GroupId}
                LEFT JOIN Users u 
                    ON fgm.{FGM_UserId} = u.{nameof(User.Id)}
                """);
        }
        if (searchUser)
        {
            query.AppendConditionLine($"""
                u.{nameof(User.Id)} = @{nameof(options.ContainsUser)}
                """);
        }
        if (options.GroupName is not null)
        {
            query.AppendConditionLine($"""
                fg.{nameof(FreshmanGroup.Name)} LIKE %@{nameof(options.GroupName)}%
                """);
        }
  

        var includeHidden = options.IncludeHiddenMembers ? "TRUE" : "FALSE";
        query.AppendConditionLine($"""
            fgm.{FGM_IsHidden} = {includeHidden}
            """);


        query.AppendLine($"""
            ORDER BY fg.{nameof(FreshmanGroup.GroupNumber)} ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);

        using var connection = await GetConnection();
        if (options.DoMinimize)
        {
            // Do not map members
            var minimized = await connection.QueryAsync<FreshmanGroup>(query.ToString(), options);
            return minimized.ToArray();
        }

        // Map members to groups
        var records = await connection.QueryAsync<FreshmanGroup, FreshmanGroupMember, FreshmanGroup>(
            query.ToString(), MapUserToGroup, options, splitOn: FGM_GroupId);

        return records
            .GroupBy(x => x.Id)
            .Select(MapGroupingToGroup)
            .ToArray();
    }



    public async Task<FreshmanGroup?> GetGroup(long id)
    {
        using var connection = await GetConnection();
        var record = await connection.QueryAsync<FreshmanGroup, FreshmanGroupMember, FreshmanGroup>($"""
            SELECT * FROM FreshmanGroups fg
            LEFT JOIN FreshmanGroupMembers fgm 
                ON fg.{nameof(FreshmanGroup.Id)} = fgm.{FGM_GroupId}

            WHERE fg.{nameof(FreshmanGroup.Id)} = @{nameof(id)}
            """, MapUserToGroup, new { id }, splitOn: FGM_GroupId);

        return record
            .GroupBy(x => x.Id)
            .Select(MapGroupingToGroup)
            .FirstOrDefault();
    }

    public async Task<Guid[]?> GetMembers(long id)
    {
        using var connection = await GetConnection();

        // Return records, no need to check if group exists because of foreign keys
        var records = await connection.QueryAsync<Guid>($"""
            SELECT {FGM_UserId} FROM FreshmanGroupMembers 
            WHERE {FGM_GroupId} = @{nameof(id)}
            """, new { id });

        return records.ToArray();
    }

    public async Task<Result<long>> Insert(FreshmanGroup record)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteScalarAsync<long>($"""
            INSERT INTO FreshmanGroups (
                {nameof(FreshmanGroup.Name)}, 
                {nameof(FreshmanGroup.LarpakeId)}, 
                {nameof(FreshmanGroup.GroupNumber)})
            VALUES (
                @{nameof(FreshmanGroup.Name)}, 
                @{nameof(FreshmanGroup.LarpakeId)}, 
                @{nameof(FreshmanGroup.GroupNumber)});
            SELECT last_insert_rowid();
            """, record);
    }

    record struct InsertModel(long Id, Guid UserId);

    public async Task<Result<int>> InsertMembers(long id, Guid[] members)
    {
        try
        {
            var records = members
                .Distinct()
                .Select(x => new InsertModel(id, x))
                .ToArray();

            using var connection = await GetConnection();
            return await connection.ExecuteAsync($"""
                INSERT OR IGNORE INTO FreshmanGroupMembers (
                    {FGM_GroupId}, 
                    {FGM_UserId})
                VALUES (
                    @{nameof(InsertModel.Id)}, 
                    @{nameof(InsertModel.UserId)});
                """, records);
        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode is SqliteError.ForeignKey_e)
        {
            return Error.NotFound("Group or user does not exist.");
        }
    }

    public async Task<Result<int>> InsertHiddenMembers(long groupId, Guid[] members)
    {
        try
        {
            var records = members
                .Distinct()
                .Select(x => new InsertModel(groupId, x));

            using var connection = await GetConnection();
            return await connection.ExecuteAsync($"""
                INSERT OR IGNORE INTO FreshmanGroupMembers (
                    {FGM_GroupId}, 
                    {FGM_UserId}, 
                    {FGM_IsHidden})
                VALUES (
                    @{nameof(InsertModel.Id)}, 
                    @{nameof(InsertModel.UserId)}, 
                    TRUE);
                """, records);

        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode is SqliteError.ForeignKey_e)
        {
            return Error.NotFound("Group or user does not exist.");
        }
    }

    public async Task<Result<int>> Update(FreshmanGroup record)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE FreshmanGroups SET
                {nameof(FreshmanGroup.Name)} = @{nameof(FreshmanGroup.Name)},
                {nameof(FreshmanGroup.GroupNumber)} = @{nameof(FreshmanGroup.GroupNumber)},
                {nameof(FreshmanGroup.UpdatedAt)} = DATETIME('now')
            WHERE {nameof(FreshmanGroup.Id)} = @{nameof(FreshmanGroup.Id)};
            """, record);
    }

    public async Task<int> Delete(long id)
    {
        using var connection = await GetConnection();

        int rowsAffected = await connection.ExecuteAsync($"""
            DELETE FROM FreshmanGroupMembers WHERE {nameof(FGM_GroupId)} = @{nameof(id)};
            """, new { id });

        return rowsAffected + await connection.ExecuteAsync($"""
            DELETE FROM FreshmanGroups WHERE {nameof(FreshmanGroup.Id)} = @{nameof(id)};
            """, new { id });
    }

    public async Task<int> DeleteMembers(long id, Guid[] memberIds)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM FreshmanGroupMembers 
            WHERE 
                {FGM_GroupId} = @{nameof(id)} 
                AND {FGM_UserId} IN @{nameof(memberIds)};
            """, new { id, memberIds });
    }



    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS FreshmanGroups (
                {nameof(FreshmanGroup.Id)} INTEGER,
                {nameof(FreshmanGroup.Name)} TEXT UNIQUE,
                {nameof(FreshmanGroup.LarpakeId)} INTEGER NOT NULL,
                {nameof(FreshmanGroup.GroupNumber)} INTEGER,
                {nameof(FreshmanGroup.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(FreshmanGroup.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY ({nameof(FreshmanGroup.LarpakeId)}) REFERENCES Larpakkeet({nameof(Larpake.Id)}),
                PRIMARY KEY ({nameof(FreshmanGroup.Id)})
            );

            CREATE TABLE IF NOT EXISTS FreshmanGroupMembers (
                {FGM_GroupId} INTEGER,
                {FGM_UserId} TEXT,
                {FGM_IsHidden} BOOL NOT NULL DEFAULT FALSE,
                PRIMARY KEY ({FGM_GroupId}, {FGM_UserId}),
                FOREIGN KEY ({FGM_GroupId}) REFERENCES FreshmanGroups({nameof(FreshmanGroup.Id)}),
                FOREIGN KEY ({FGM_UserId}) REFERENCES Users({nameof(User.Id)})
            );
            """);
    }

    #region NAMEOF_CONSTANTS
    private const string FGM_GroupId = nameof(FreshmanGroupMember.GroupId);
    private const string FGM_UserId = nameof(FreshmanGroupMember.UserId);
    private const string FGM_IsHidden = nameof(FreshmanGroupMember.IsHidden);
    #endregion NAMEOF_CONSTANTS

    private static FreshmanGroup MapGroupingToGroup(IGrouping<long, FreshmanGroup> grouping)
    {
        FreshmanGroup group = grouping.First();
        group.Members = grouping
            .Where(g => g.Members is not null)
            .SelectMany(y => y.Members!)
            .Distinct()
            .ToList();
        return group;
    }

    private static FreshmanGroup MapUserToGroup(FreshmanGroup group, FreshmanGroupMember member)
    {
        // Add member to group object
        group.Members ??= [];
        if (member is null)
        {
            return group;
        }
        group.Members.Add(member.UserId);
        return group;
    }

    Task<FreshmanGroup[]> IFreshmanGroupDatabase.GetGroupsMinimized(FreshmanGroupQueryOptions options)
    {
        throw new NotImplementedException();
    }
}
