using LarpakeServer.Models.External;
using LarpakeServer.Services.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace LarpakeServer.Services.Implementations;

public class LuuppiIntegrationService : IExternalIntegrationService
{
    protected record JsonEvent(ExternalEvent[] Events);
    protected record JsonUser(ExternalUserInformation User);

    readonly IHttpClientFactory _clientFactory;
    readonly ILogger<LuuppiIntegrationService> _logger;
    private readonly IntegrationOptions _options;
    readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    public LuuppiIntegrationService(
        IHttpClientFactory clientFactory, 
        ILogger<LuuppiIntegrationService> logger,
        IOptions<IntegrationOptions> options
        )
    {
        Guard.ThrowIfNull(clientFactory);
        _clientFactory = clientFactory;
        _logger = logger;
        _options = options.Value;
    }

    /* Implementation assumes that
     * - Api key is provided during the injection of the service
     * - Base url is provided during the injection of the service and does not end '/' (path separator)
     */

    public async Task<Result<ExternalEvent[]>> PullEventsFromExternalSource(CancellationToken token)
    {
        // Make request
        HttpClient client = _clientFactory.CreateClient(Constants.HttpClients.IntegrationClient);   // Factory manages disposal
        HttpResponseMessage request = await client.GetAsync("/api/integration/event-feed", token);
        if (request.IsSuccessStatusCode is false)
        {
            _logger.LogWarning("External event server failed to respond with status '{code}' and message: {msg}.",
                request.StatusCode, request.RequestMessage);
            return Error.InternalServerError($"External server failed to respond with status '{request.StatusCode}'",
                ErrorCode.ExternalServerError);
        }

        // Map json
        
        using Stream json = await request.Content.ReadAsStreamAsync(token);

        JsonEvent? result = await JsonSerializer.DeserializeAsync<JsonEvent>(json, _jsonOptions, token);
        if (result is null)
        {
            _logger.LogError("Invalid data returned from external server.");
            return Error.InternalServerError(
                "External server returned invalid data.", ErrorCode.ExternalServerError);
        }
        return result.Events;
    }

    public async Task<Result<ExternalUserInformation>> PullUserInformationFromExternalSource(Guid userId, CancellationToken token)
    {
        if (userId == Guid.Empty)
        {
            return Error.BadRequest("User id cannot be empty guid.", ErrorCode.InvalidId);
        }


        // Make request
        HttpClient client = _clientFactory.CreateClient(Constants.HttpClients.IntegrationClient); // Factory manages disposal
        HttpResponseMessage request = await client.GetAsync($"/api/integration/user-info?userId={userId}", token);
        if (request.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("User not found.", ErrorCode.IdNotFound);
        }

        string json = await request.Content.ReadAsStringAsync();

        if (request.IsSuccessStatusCode is false)
        {
            _logger.LogWarning("External user server failed to respond with status '{code}' and message: {msg}.",
                request.StatusCode, request.RequestMessage);

            return Error.InternalServerError($"External user server failed to respond with status '{request.StatusCode}'",
                ErrorCode.ExternalServerError);
        }

        // Map json
        JsonUser? data = await request.Content.ReadFromJsonAsync<JsonUser>(_jsonOptions, token);
        if (data is null)
        {
            _logger.LogError("Invalid data returned from external user server.");
            return Error.InternalServerError("External user server returned invalid data.",
                ErrorCode.ExternalServerError);
        }
        return data.User;
    }


}
