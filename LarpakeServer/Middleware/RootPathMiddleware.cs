namespace LarpakeServer.Middleware;

public class RootPathMiddleware
{
    readonly RequestDelegate _next;

    public RootPathMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Value is null)
        {
            await _next(context);
            return;
        }
        ReadOnlySpan<char> path = context.Request.Path.Value.AsSpan();
        if (path is "/en/" || path is "/en")
        {
            context.Response.Redirect("en/index", true);
            return;
        }
        if (path is "" || path is "/" || path is "/fi/" || path is "/fi")
        {
            // Redirect to /fi
            context.Response.Redirect("fi/index", true);
            return;
        }
        await _next(context);
    }
}
