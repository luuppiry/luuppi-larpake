using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class FreshmanGroupDatabase(SqliteConnectionString connectionString, 
    UserDatabase userDb) 
    : SqliteDbBase(connectionString, userDb), IFreshmanGroupDatabase
{
 

    public Task<FreshmanGroup[]> Get(FreshmanGroupQueryOptions options)
    {
        throw new NotImplementedException();
    }

    public Task<FreshmanGroup?> Get(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Guid[]?> GetMembers(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<long>> Insert(FreshmanGroup record)
    {
        throw new NotImplementedException();
    }

    public Task<Result<long>> InsertMembers(long[] memberIds)
    {
        throw new NotImplementedException();
    }

    public Task<Result<int>> Update(FreshmanGroup record)
    {
        throw new NotImplementedException();
    }

    public Task<int> Delete(long groupId)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteMembers(long groupId, Guid[] memberIds)
    {
        throw new NotImplementedException();
    }

    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""

            CREATE TABLE IF NOT EXISTS FreshmanGroups (
                {nameof(FreshmanGroup.Id)} INTEGER,
                {nameof(FreshmanGroup.Name)} TEXT UNIQUE,
                {nameof(FreshmanGroup.StartYear)} INTEGER NOT NULL,
                {nameof(FreshmanGroup.GroupNumber)} INTEGER,
                {nameof(FreshmanGroup.CreatedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(FreshmanGroup.LastModifiedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(FreshmanGroup.Id)})
            );

            CREATE TABLE IF NOT EXISTS FreshmanGroupMembers (
                {nameof(FreshmanGroupMember.FreshmanGroupId)} INTEGER,
                {nameof(FreshmanGroupMember.UserId)} TEXT,
                PRIMARY KEY ({nameof(FreshmanGroupMember.FreshmanGroupId)}, {nameof(FreshmanGroupMember.UserId)}),
                FOREIGN KEY ({nameof(FreshmanGroupMember.FreshmanGroupId)}) REFERENCES FreshmanGroups({nameof(FreshmanGroup.Id)}),
                FOREIGN KEY ({nameof(FreshmanGroupMember.UserId)}) REFERENCES Users({nameof(User.Id)})
            );
            """);
    }
}
