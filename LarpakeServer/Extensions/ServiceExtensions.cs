using LarpakeServer.Data;
using LarpakeServer.Data.Sqlite;
using LarpakeServer.Helpers;
using LarpakeServer.Services;
using LarpakeServer.Services.Implementations;
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

    public static void AddSqliteDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new SqliteConnectionString(configuration["ConnectionStrings:Sqlite"]!));
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        
        services.AddSingleton<IEventDatabase, EventDatabase>();
        services.AddSingleton<EventDatabase>();
        services.AddSingleton<IUserDatabase, UserDatabase>();
        services.AddSingleton<UserDatabase>();
        services.AddSingleton<IFreshmanGroupDatabase, FreshmanGroupDatabase>();
        services.AddSingleton<FreshmanGroupDatabase>();
        services.AddSingleton<IAttendanceDatabase, AttendanceDatabase>();
        services.AddSingleton<AttendanceDatabase>();
        services.AddSingleton<ISignatureDatabase, SignatureDatabase>();
        services.AddSingleton<SignatureDatabase>();
    }

    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<CompletionMessageService>();
        services.AddSingleton<ISubscriptionClientPool, InMemorySubscriptionClientService>();
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
