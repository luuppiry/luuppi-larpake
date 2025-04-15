namespace LarpakeServer.Middleware;

public class LanguageMiddleware
{
    readonly RequestDelegate _next;
    readonly IWebHostEnvironment _env;

    public LanguageMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Value is null)
        {
            await _next(context);
            return;
        }
        ReadOnlySpan<char> path = context.Request.Path.Value.AsSpan();

        /* Check if path meets conditions
        * - No api endpoint /api 
        * - Does not have language /fi or /en
        * - Has .html extension or no extension
        * - Does not route to scalar or openapi
        */

        bool isOpenapi = path.StartsWith("/openapi") || path.StartsWith("/scalar");
        bool isApi = path.StartsWith("/api");
        bool hasLanguage = path.StartsWith("/fi") || path.StartsWith("/en");

        if (isOpenapi || isApi || hasLanguage)
        {
            await _next(context);
            return;
        }

        // Check if path is html file
        bool isHtmlFile = path.EndsWith(".html") || MiddlewareHelpers.IsExtensionless(path);
        if (isHtmlFile is false)
        {
            await _next(context);
            return;
        }

        // Redirect to finnish page
        QueryString query = context.Request.QueryString;
        string newPath = $"/fi{path}{query}";

        context.Response.Redirect(newPath, true);

    }
}
