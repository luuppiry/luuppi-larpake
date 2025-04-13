namespace LarpakeServer.Middleware;

public static class StaticFileServer
{
    public static WebApplication AddFrontendServing(this WebApplication app, ILogger<DITypeMarker> logger)
    {
        app.UseStaticFiles(new StaticFileOptions()
        {
            OnPrepareResponse = ctx =>
            {
                if (ctx.File?.Name is null)
                {
                    return;
                }
                if (ctx.File.Name.EndsWith(".html"))
                {
                    ctx.Context.Response.ContentType = "text/html; charset=utf-8";
                    return;
                }
                if (ctx.File.Name.EndsWith(".js"))
                {
                    ctx.Context.Response.ContentType = "application/javascript";
                    return;
                }
                if (ctx.File.Name.EndsWith(".css"))
                {
                    ctx.Context.Response.ContentType = "text/css";
                    return;
                }
            }
        });

        return app;
    }
}
