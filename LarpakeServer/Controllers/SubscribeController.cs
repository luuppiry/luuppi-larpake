using LarpakeServer.Models.EventModels;
using LarpakeServer.Services;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/sse/[controller]")]
public class SubscribeController : ControllerBase
{
    readonly CompletionMessageService _messageService;
    readonly IClientPool _clients;
    readonly ILogger<SubscribeController> _logger;
    static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public SubscribeController(
        CompletionMessageService service,
        IClientPool clients,
        ILogger<SubscribeController> logger)
    {
        _logger = logger;
        _clients = clients;
        _messageService = service;
        _messageService.TaskReceived += async (_, e)=> await HandleTask(e);
    }


    [HttpGet("{clientId}")]
    public async Task<IActionResult> Subscribe(Guid clientId, CancellationToken token)
    {
        if (clientId == Guid.Empty)
        {
            _logger.LogInformation("");
            return BadRequest(new
            {
                Message = "Invalid client id",
                Details = "Client id cannot be empty, to generate one see UUIDv7"
            });
        }



        // Add client to pool
        var client = Response;
        var insertStatus = _clients.Add(clientId, client);

        if (insertStatus is not PoolInsertStatus.Success)
        {
            return insertStatus switch
            {
                PoolInsertStatus.Full => StatusCode(503, new
                {
                    Message = "Service Unavailable",
                    Details = "Server is currently full, try again later"
                }),
                _ => throw new InvalidOperationException($"Invalid {nameof(PoolInsertStatus)}")
            };
        }


        // Open SSE connection
        Response.Headers.Append("Content-Type", "text/event-stream");


        // Maintain connection
        try
        {
            while (HttpContext.RequestAborted.IsCancellationRequested is false)
            {
                await Task.Delay(1000, token);
            }
        }
        catch (TaskCanceledException) { }

        // Close connection
        if (_clients.Remove(clientId, client) is false)
        {
            _logger.LogWarning("Client {clientId} was not removed from the pool.", clientId);
        }
        return Ok();
    }

    private async Task HandleTask(AttendedCreated e)
    {
        // jsonify event
        var json = JsonSerializer.Serialize((Completed)e, _jsonOptions);

        // validate json
        while (json.Contains("\n\n"))
        {
#if DEBUG
            throw new InvalidOperationException(
                "SSE Message contains invalid sequence of line breaks '\\n\\n'");
#else
            _logger.LogWarning("SSE Message contained invalid sequence '\\n\\n': {json}", json);
            json = json.Replace("\n\n", "\n");
#endif
        }

        // format message
        var message = $"data: {json}\n\n";

        // find target client or message all
        if (_clients.TryFind(e.TargetClientId, out var idClient))
        {
            await idClient.WriteAsync($"data: {json}\n\n");
            await idClient.Body.FlushAsync();
            return;
        }

        foreach (var nonIdClient in _clients.GetAll())
        {
            await nonIdClient.WriteAsync(message);
            await nonIdClient.Body.FlushAsync();
        }
    }

    class Completed
    {
        public required Guid UserId { get; init; }
        public required long EventId { get; init; }
        public required Guid CompletionId { get; init; }

        public const string Status = Constants.CompletedStatusString;
        public const string Message = "Attendance completed";

        public static implicit operator Completed(AttendedCreated e) => new()
        {
            UserId = e.UserId,
            EventId = e.EventId,
            CompletionId = e.CompletionId
        };
    }
}
