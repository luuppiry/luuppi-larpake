namespace LarpakeServer.Middleware;

public static class StaticFileServer
{
    public static WebApplication AddFrontendServing(this WebApplication app)
    {
        app.UseStaticFiles();
        app.MapFallbackToFile("fi/index.html");
        return app;
    }
}
