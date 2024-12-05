using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;

public interface IEventDatabase
{
    Task<Event[]> Get(EventQueryOptions options);
    Task<Event?> Get(long id);
    Task<long> Insert(Event record);
}
