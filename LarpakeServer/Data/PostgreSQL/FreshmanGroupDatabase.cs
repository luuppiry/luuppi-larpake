using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data.PostgreSQL;

public class FreshmanGroupDatabase(NpgsqlConnectionString connectionString) : PostgresDb(connectionString), IFreshmanGroupDatabase
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

    public Task<Result<int>> InsertHiddenMembers(long groupId, Guid[] members)
    {
        throw new NotImplementedException();
    }

    public Task<Result<int>> InsertMembers(long id, Guid[] members)
    {
        throw new NotImplementedException();
    }

    public Task<Result<int>> Update(FreshmanGroup record)
    {
        throw new NotImplementedException();
    }

    public Task<int> Delete(long id)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteMembers(long id, Guid[] members)
    {
        throw new NotImplementedException();
    }
}
