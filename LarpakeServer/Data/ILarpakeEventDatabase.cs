using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;
public interface ILarpakeEventDatabase
{
    Task<LarpakeEvent?> GetEvent(long id);
    Task<LarpakeEvent[]> GetEvents(LarpakeEventQueryOptions options);
    Task<Result<long>> Insert(LarpakeEvent record);
    Task<Result> SyncOrganizationEvent(long larpakeEventId, long organizationEventId);
    Task<int> UnsyncOrganizationEvent(long larpakeEventId, long organizationEventId);
    Task<Result<int>> Update(LarpakeEvent record);
    Task<int> Cancel(long id);
}