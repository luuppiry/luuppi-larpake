using LarpakeServer.Data;
using LarpakeServer.Data.Sqlite;
using LarpakeServer.Data.Sqlite.TypeHandlers;
using LarpakeServer.Identity;
using LarpakeServer.Services;
using LarpakeServer.Services.Implementations;

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
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler());
        
        services.AddSingleton<IEventDatabase, OrganizationEventDatabase>();
        services.AddSingleton<OrganizationEventDatabase>();
        services.AddSingleton<IUserDatabase, UserDatabase>();
        services.AddSingleton<UserDatabase>();
        services.AddSingleton<IFreshmanGroupDatabase, FreshmanGroupDatabase>();
        services.AddSingleton<FreshmanGroupDatabase>();
        services.AddSingleton<IAttendanceDatabase, AttendanceDatabase>();
        services.AddSingleton<AttendanceDatabase>();
        services.AddSingleton<ISignatureDatabase, SignatureDatabase>();
        services.AddSingleton<SignatureDatabase>();
        services.AddSingleton<IRefreshTokenDatabase, RefreshTokenDatabase>();
        services.AddSingleton<RefreshTokenDatabase>();
        services.AddSingleton<ILarpakeDatabase, LarpakeDatabase>();
        services.AddSingleton<LarpakeDatabase>();
        services.AddSingleton<ILarpakeEventDatabase, LarpakeEventDatabase>();
        services.AddSingleton<LarpakeEventDatabase>();
    }

    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<CompletionMessageService>();
        services.AddSingleton(new ClientPoolConfiguration
        {
            MaxSize = configuration.GetValue<int>("SSE:InMemoryClientPoolSize")
        });
        services.AddSingleton<IClientPool, InMemoryClientPool>();
        services.AddSingleton<TokenService>();
        services.AddSingleton<IClaimsReader, TokenService>();
    }
    
}
