﻿using LarpakeServer.Identity;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Services;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/sse/subscribe")]
public class SubscribeController : ExtendedControllerBase
{
    readonly CompletionMessageService _messageService;
    readonly IClientPool _clientPool;
    static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public SubscribeController(
        CompletionMessageService service,
        IClientPool clients,
        ILogger<SubscribeController> logger,
        IClaimsReader claimsReader
        ) : base(claimsReader, logger)
    {
        _clientPool = clients;
        _messageService = service;
        _messageService.TaskReceived += async (_, e) => await HandleTask(e);
    }


    [HttpGet]
    [RequiresPermissions(Permissions.CommonRead)]
    public async Task<IActionResult> Subscribe(CancellationToken token)
    {
        Guid userId = GetRequestUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest("Invalid user id", "User id cannot be empty", ErrorCode.NullId);
        }

        // Add client to pool
        var client = Response;
        var insertStatus = _clientPool.Add(userId, client);

        if (insertStatus is not PoolInsertStatus.Success)
        {
            return insertStatus switch
            {
                PoolInsertStatus.Full => StatusCode(503, new ErrorMessageResponse
                {
                    Message = "Service Unavailable",
                    Details = "SSE server is currently full, try again later",
                    ApplicationError = ErrorCode.SSEError
                }),
                PoolInsertStatus.Blocked => StatusCode(403, new ErrorMessageResponse
                {
                    Message = "Forbidden",
                    Details = "User is already connected to the SSE server",
                    ApplicationError = ErrorCode.MaxUserConnections
                }),
                PoolInsertStatus.Failed => InternalServerError("Internal Server Error"),
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
        catch (TaskCanceledException)
        {
        }

        // Close connection
        bool isRemoved = _clientPool.Remove(userId, client);

        _logger.IfFalse(isRemoved)
               .LogInformation("Client {clientId} was not removed from the pool.", userId);

        return Ok();
    }

    private async Task HandleTask(AttendedCreated e)
    {
        _logger.LogTrace("New SSE message for user {userId}.", e.UserId);

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

        // Try find client from id, if not found send to all
        HttpResponse[]? clients;
        clients = _clientPool.TryFind(e.UserId, out clients)
            ? clients
            : _clientPool.GetAll();

        // Send message to chosen clients
        foreach (var client in clients)
        {
            await client.WriteAsync(message);
            await client.Body.FlushAsync();
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
            EventId = e.LarpakeTaskId,
            CompletionId = e.CompletionId
        };
    }
}
