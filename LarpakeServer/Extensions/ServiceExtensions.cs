using LarpakeServer.Data;
using LarpakeServer.Data.Sqlite;
using System.Text.Json;

namespace LarpakeServer.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    public static void AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEventDatabase, EventDatabase>();
    }

    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {

    }
    
    public static void AddLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddJsonConsole(config => 
            {
#if DEBUG   
                    config.JsonWriterOptions = new JsonWriterOptions
                    {
                        Indented = true
                    };
#endif
            });
        });
    }


}
