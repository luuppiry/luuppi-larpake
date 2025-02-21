using LarpakeServer.Models.External;

namespace LarpakeServer.Services;

public interface IExternalIntegrationService
{
    Task<Result<ExternalEvent[]>> PullEventsFromExternalSource(CancellationToken token);
    Task<Result<ExternalUserInformation>> PullUserInformationFromExternalSource(Guid userId, CancellationToken token);
}
