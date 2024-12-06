using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;

public interface IEventDatabase
{
    Task<Event[]> Get(EventQueryOptions options);
    Task<Event?> Get(long id);
    Task<Result<long>> Insert(Event record);
    Task<Result<int>> Update(Event record);
    Task<int> Delete(long eventId, Guid modifyingUser);
}
