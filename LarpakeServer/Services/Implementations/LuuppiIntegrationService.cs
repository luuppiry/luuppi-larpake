using LarpakeServer.Models.External;

namespace LarpakeServer.Services.Implementations;

public class LuuppiIntegrationService : IExternalIntegrationService
{
    readonly IHttpClientFactory _clientFactory;
    readonly ILogger<LuuppiIntegrationService> _logger;

    public LuuppiIntegrationService(IHttpClientFactory clientFactory, ILogger<LuuppiIntegrationService> logger)
    {
        Guard.ThrowIfNull(clientFactory);
        _clientFactory = clientFactory;
        _logger = logger;
    }

    /* Implementation assumes that
     * - Api key is provided during the injection of the service
     * - Base url is provided during the injection of the service and does not end '/' (path separator)
     */

    public async Task<Result<ExternalEvent[]>> PullEventsFromExternalSource(CancellationToken token)
    {
        // Make request
        using HttpClient client = _clientFactory.CreateClient(Constants.HttpClients.IntegrationClient);
        HttpResponseMessage request = await client.GetAsync("/api/integration/event-feed", token);
        if (request.IsSuccessStatusCode is false)
        {
            _logger.LogWarning("External event server failed to respond with status '{code}' and message: {msg}.",
                request.StatusCode, request.RequestMessage);
            return Error.InternalServerError($"External server failed to respond with status '{request.StatusCode}'",
                ErrorCode.ExternalServerError);
        }

        // Map json
        IEnumerable<ExternalEvent>? events = await request.Content.ReadFromJsonAsync<IEnumerable<ExternalEvent>>(token);
        if (events is null)
        {
            _logger.LogError("Invalid data returned from external event server.");

            return Error.InternalServerError("External event server returned invalid data.",
                ErrorCode.ExternalServerError);
        }
        return events.ToArray();
    }

    public async Task<Result<ExternalUserInformation>> PullUserInformationFromExternalSource(Guid userId, CancellationToken token)
    {
        if (userId == Guid.Empty)
        {
            return Error.BadRequest("User id cannot be empty guid.", ErrorCode.InvalidId);
        }


        // Make request
        using HttpClient client = _clientFactory.CreateClient(Constants.HttpClients.IntegrationClient);
        HttpResponseMessage request = await client.GetAsync($"/api/integration/user?userId={userId}", token);
        if (request.IsSuccessStatusCode is false)
        {
            _logger.LogWarning("External user server failed to respond with status '{code}' and message: {msg}.",
                request.StatusCode, request.RequestMessage);

            return Error.InternalServerError($"External user server failed to respond with status '{request.StatusCode}'",
                ErrorCode.ExternalServerError);
        }

        // Map json
        ExternalUserInformation? data = await request.Content.ReadFromJsonAsync<ExternalUserInformation>(token);
        if (data is null)
        {
            _logger.LogError("Invalid data returned from external user server.");
            return Error.InternalServerError("External user server returned invalid data.",
                ErrorCode.ExternalServerError);
        }
        return data;
    }
}
