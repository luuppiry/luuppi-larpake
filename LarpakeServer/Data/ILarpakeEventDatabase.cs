using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Data;
public interface ILarpakeEventDatabase
{
    Task<LarpakeEvent?> GetEvent(long id);
    Task<LarpakeEvent[]> GetLarpakeEvents(long larpakeId);
    Task<LarpakeEvent[]> GetSectionEvents(long sectionId);
    Task<long> Insert(LarpakeEvent record);
    Task<Result> SyncOrganizationEvent(long larpakeEventId, long organizationEventId);
    Task<int> UnsyncOrganizationEvent(long larpakeEventId, long organizationEventId);
    Task<int> Update(LarpakeEvent record);
}