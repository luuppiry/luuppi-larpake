using LarpakeServer.Helpers.Generic;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;

public interface IOrganizationEventDatabase
{
    Task<OrganizationEvent[]> Get(EventQueryOptions options);
    Task<OrganizationEvent?> Get(long id);
    Task<Result<long>> Insert(OrganizationEvent record);
    Task<Result<int>> Update(OrganizationEvent record);
    Task<int> SoftDelete(long eventId, Guid modifyingUser);
    Task<int> HardDelete(long eventId);
}
