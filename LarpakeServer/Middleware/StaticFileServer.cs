namespace LarpakeServer.Middleware;

public static class StaticFileServer
{
    public static WebApplication AddFrontendServing(this WebApplication app)
    {
        app.UseStaticFiles();

        app.UseStatusCodePages((builder) =>
        {
            var ctx = builder.HttpContext;

            // Validate that request is not found and not for an API endpoint 
            if (ctx.Response.StatusCode is not 404 || ctx.Request.Path.StartsWithSegments("/api"))
            {
                return Task.CompletedTask;
            }

            // Return index or not found
            string path = ctx.Request.Path;
            ctx.Response.Redirect(path switch
            {
                "/" or "/fi" => "/fi/index.html",
                "/en" => "/en/index.html",
                _ => "/fi/404.html"
            });
            return Task.CompletedTask;
        });
        return app;
    }
}
