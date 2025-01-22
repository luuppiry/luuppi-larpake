using Sqlite = LarpakeServer.Data.Sqlite;
using Postgres = LarpakeServer.Data.PostgreSQL;
using LarpakeServer.Data;
using LarpakeServer.Data.TypeHandlers;
using LarpakeServer.Identity;
using LarpakeServer.Services;
using LarpakeServer.Services.Implementations;
using static LarpakeServer.Services.AttendanceKeyService;

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
        services.AddSingleton(new SqliteConnectionString(configuration.GetConnectionString("Sqlite")!));
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler());
        
        services.AddSingleton<IOrganizationEventDatabase, Sqlite.OrganizationEventDatabase>();
        services.AddSingleton<Sqlite.OrganizationEventDatabase>();
        services.AddSingleton<IUserDatabase, Sqlite.UserDatabase>();
        services.AddSingleton<Sqlite.UserDatabase>();
        services.AddSingleton<IFreshmanGroupDatabase, Sqlite.FreshmanGroupDatabase>();
        services.AddSingleton<Sqlite.FreshmanGroupDatabase>();
        services.AddSingleton<IAttendanceDatabase, Sqlite.AttendanceDatabase>();
        services.AddSingleton<Sqlite.AttendanceDatabase>();
        services.AddSingleton<ISignatureDatabase, Sqlite.SignatureDatabase>();
        services.AddSingleton<Sqlite.SignatureDatabase>();
        services.AddSingleton<IRefreshTokenDatabase, Sqlite.RefreshTokenDatabase>();
        services.AddSingleton<Sqlite.RefreshTokenDatabase>();
        services.AddSingleton<ILarpakeDatabase, Sqlite.LarpakeDatabase>();
        services.AddSingleton<Sqlite.LarpakeDatabase>();
        services.AddSingleton<ILarpakeEventDatabase, Sqlite.LarpakeEventDatabase>();
        services.AddSingleton<Sqlite.LarpakeEventDatabase>();
    }
    
    public static void AddSPostgresDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new NpgsqlConnectionString(configuration.GetConnectionString("PostgreSQL")!));
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler());
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddSingleton<IOrganizationEventDatabase, Postgres.OrganizationEventDatabase>();
        services.AddSingleton<IUserDatabase, Postgres.UserDatabase>();
        services.AddSingleton<IFreshmanGroupDatabase, Postgres.FreshmanGroupDatabase>();
        services.AddSingleton<IAttendanceDatabase, Postgres.AttendanceDatabase>();
        services.AddSingleton<ISignatureDatabase, Postgres.SignatureDatabase>();
        services.AddSingleton<IRefreshTokenDatabase, Postgres.RefreshTokenDatabase>();
        services.AddSingleton<ILarpakeDatabase, Postgres.LarpakeDatabase>();
        services.AddSingleton<ILarpakeEventDatabase, Postgres.LarpakeEventDatabase>();
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
        services.AddSingleton<AttendanceKeyService>();
        services.AddSingleton(new AttendanceKeyServiceOptions
        {
            KeyLength = configuration.GetValue<int>("AttendanceKey:KeyLength")!,
            KeyLifetimeHours = configuration.GetValue<int>("AttendanceKey:KeyLifetimeHours")!
        });
    }
    
}
