using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;
public interface ILarpakeEventDatabase
{
    Task<LarpakeEvent?> GetEvent(long id);
    Task<LarpakeEvent[]> GetEvents(LarpakeEventQueryOptions options);
    Task<long> Insert(LarpakeEvent record);
    Task<Result> SyncOrganizationEvent(long larpakeEventId, long organizationEventId);
    Task<int> UnsyncOrganizationEvent(long larpakeEventId, long organizationEventId);
    Task<int> Update(LarpakeEvent record);
    Task<int> Cancel(long id);
}