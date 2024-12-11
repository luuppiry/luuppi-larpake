using LarpakeServer.Models.EventModels;
using LarpakeServer.Services;
using System.Text.Json;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscribeController : ControllerBase
{
    readonly CompletionMessageService _messageService;
    readonly ISubscriptionClientPool _clients;
    readonly ILogger<SubscribeController> _logger;
    readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };


    public SubscribeController(
        CompletionMessageService service, 
        ISubscriptionClientPool clients,
        ILogger<SubscribeController> logger
        )
    {
        _logger = logger;
        _messageService = service;
        _clients = clients;

        _messageService.TaskReceived += async (sender, e) =>
        {
            if (e.TargetClientId is not null)
            {
                var client = _clients.TryFind(e.TargetClientId.Value);
                if (client is not null)
                {
                    await Message(client, e);
                    return;
                }
            }
            await MessageAll(e);
        };
    }



    [HttpGet]
    public async Task Get()
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        _clients.Add(Response, null);
        while (HttpContext.RequestAborted.IsCancellationRequested is false)
        {
            await Task.Delay(1000);
        }
        _clients.Remove(Response);
    }

    [HttpGet("{clientId}")]
    public async Task Get(Guid clientId)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        _clients.Add(Response, clientId);
        while (HttpContext.RequestAborted.IsCancellationRequested is false)
        {
            await Task.Delay(1000);
        }
        _clients.Remove(Response);
    }




    private async Task MessageAll(AttendedCreated e)
    {
        object data = new
        {
            e.UserId,
            e.EventId,
            e.CompletionId,
            Message = "Attendance completed"
        };
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        while (json.Contains("\n\n"))
        {
#if DEBUG
            throw new InvalidOperationException(
                "Message contains invalid characters");
#else
            _logger.LogError("SSE Message contained invalid characters (2xline break): {json}", json);
            json = json.Replace("\n\n", "\n");
#endif
        }
        var message = $"data: {json}\n\n";

        foreach (var client in _clients.GetAll())
        {
            await client.WriteAsync(message);
            await client.Body.FlushAsync();
        }
    }

    private static async Task Message(HttpResponse client, AttendedCreated e)
    {
        object data = new
        {
            e.UserId,
            e.EventId,
            e.CompletionId,
            Message = "Attendance completed"
        };
        var json = JsonSerializer.Serialize(data);
        await client.WriteAsync($"data: {json}\n\n");
        await client.Body.FlushAsync();
    }
}
