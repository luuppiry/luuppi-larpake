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

        //app.UseStatusCodePages((builder) =>
        //{
        //    var ctx = builder.HttpContext;

        //    // Validate that request is not found and not for an API endpoint 
        //    if (ctx.Response.StatusCode is not 404 || ctx.Request.Path.StartsWithSegments("/api"))
        //    {
        //        return Task.CompletedTask;
        //    }

        //    // Return index or not found
        //    string path = ctx.Request.Path;
        //    ctx.Response.Redirect(path switch
        //    {
        //        "/" or "/fi" => "/fi/index.html",
        //        "/en" => "/en/index.html",
        //        _ => "/fi/404.html"
        //    });
        //    return Task.CompletedTask;
        //});
        return app;
    }
}
