using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;
public interface ILarpakeTaskDatabase
{
    Task<LarpakeTask?> GetTask(long id);
    Task<LarpakeTask[]> GetTasks(LarpakeTaskQueryOptions options);
    Task<Result<long>> Insert(LarpakeTask record);
    Task<Result> SyncOrganizationEvent(long larpakeTaskId, long organizationEventId);
    Task<int> UnsyncOrganizationEvent(long larpakeTaskId, long organizationEventId);
    Task<Result<int>> Update(LarpakeTask record);
    Task<int> Cancel(long id);
    Task<long[]> GetRefOrganizationEvents(long id);

}