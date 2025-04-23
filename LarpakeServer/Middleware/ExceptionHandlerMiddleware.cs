using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
using System.Text.Json;

namespace LarpakeServer.Middleware;

public class ExceptionHandlerMiddleware : IExceptionHandler
{
    class ExceptionResponse
    {
        public required string Message { get; set; }
        public required Guid TraceId { get; set; }
        public string Details { get; set; } = "";
        public required ErrorCode StatusCode { get; set; }
    }

    readonly RequestDelegate _next;
    readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (await TryHandleAsync(context, ex, CancellationToken.None))
            {
                return;
            }
            throw;
        }
    }


    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        Guid traceId = Guid.NewGuid();
        string path = httpContext.Request.Path;
        string query = httpContext.Request.QueryString.ToString();
        HttpResponse response = httpContext.Response;

        // Only handle exceptions from API calls
        if (path.StartsWith("/api") is false)
        {
            return false;
        }

        response.StatusCode = StatusCodes.Status500InternalServerError;
        response.ContentType = "application/json";


        /* Log exception with trace id and details
         * Also return error message to client with same trace id
         * Empty line int the log message is left for readability */
        string json;
        switch (exception)
        {
            case NpgsqlException postgresEx:
                // Database error
                _logger.LogError(postgresEx, """
                    Unhandled Database Exception thrown
                        Request path: {path}
                        Query string: {query}
                        Type: {type}
                        TraceId: {traceId}
                        Message: {message}
                        SqlState: {sqlState}

                    """, path, query, postgresEx.GetType(), traceId,
                    postgresEx.Message, postgresEx.SqlState);

                json = JsonSerializer.Serialize(new ExceptionResponse
                {
                    Message = "Internal database error",
                    TraceId = traceId,
                    StatusCode = ErrorCode.DatabaseError
                });
                await response.WriteAsync(json, cancellationToken);
                return true;

            case Exception ex:
                _logger.LogError(ex, """
                    Unhandled Exception thrown
                        Request path: {path}
                        Query string: {query}
                        Type: {type}
                        TraceId: {traceId}
                        Message: {message}

                    """, path, query, ex.GetType(), traceId, ex.Message);
                break;

            default:
                _logger.LogError("""
                    Unhandled null thrown
                        Request path: {path}
                        Query string: {query}
                        Type: <null>
                        TraceId: {traceId}
                        Message: <null>

                    """, path, query, traceId);
                break;
        }

        json = JsonSerializer.Serialize(new ExceptionResponse
        {
            Message = "Internal server error",
            TraceId = traceId,
            StatusCode = ErrorCode.UnknownServerError
        });
        await response.WriteAsync(json, cancellationToken);
        return true;
    }
}
