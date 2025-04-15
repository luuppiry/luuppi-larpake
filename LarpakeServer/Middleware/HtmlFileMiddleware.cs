namespace LarpakeServer.Middleware;

public sealed class HtmlFileMiddleware
{
    readonly RequestDelegate _next;
    readonly IWebHostEnvironment _env;

    public HtmlFileMiddleware(RequestDelegate next, IWebHostEnvironment env)
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
        if (path.EndsWith(".js") is false)
        {
            Console.WriteLine(path.ToString());
        }

        /* Check if path meets conditions
         * - No api endpoint /api 
         * - Has language /fi or /en 
         * - Has .html extension or no extension
         */
        if (IsMiddlewareFile(path) is false)
        {
            await _next(context);
            return;
        }

        // File is html page and should be handled
        ReadOnlySpan<char> html = stackalloc char[] { '.', 'h', 't', 'm', 'l' };
        if (path.EndsWith(html))
        {
            // Remove .html extensions and redirect to non html endpoint
            QueryString query = context.Request.QueryString;
            string newPath = $"{path[..^html.Length]}{query}";

            context.Response.Redirect(newPath, true);
            await _next(context);
            return;
        }

        // Set .html extension (server side) to locate correct file
        context.Request.Path = $"{path}.html";
        await _next(context);
    }

    private static bool IsMiddlewareFile(ReadOnlySpan<char> path)
    {
        bool isApi = path.StartsWith("/api");
        bool isFinnish = path.StartsWith("/fi");
        bool isEnglish = path.StartsWith("/en");
        bool isHtmlFile = path.EndsWith(".html") || IsExtensionless(path);

        return isApi is false && (isFinnish || isEnglish) && isHtmlFile;

    }

    private static bool IsExtensionless(ReadOnlySpan<char> path)
    {
        for (int i = path.Length - 1; i >= 0; i--)
        {
            if (path[i] is '.')
            {
                return false;
            }
            if (path[i] is '/')
            {
                return true;
            }
        }
        return true;
    }

}
