using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;

public interface IFreshmanGroupDatabase
{
    Task<FreshmanGroup[]> Get(FreshmanGroupQueryOptions options);
    Task<FreshmanGroup?> Get(long id);
    Task<Guid[]?> GetMembers(long id);
    Task<Result<long>> Insert(FreshmanGroup record);
    Task<Result<long>> InsertMembers(long[] memberIds);
    Task<Result<int>> Update(FreshmanGroup record);
    Task<int> Delete(long groupId);
    Task<int> DeleteMembers(long groupId, Guid[] memberIds);
}
