using LarpakeServer.Models.External;

namespace LarpakeServer.Data;

public interface IExternalDataDbService
{
    Task<Result<int>> SyncExternalEvents(ExternalEvent[] events);
}