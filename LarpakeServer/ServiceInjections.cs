using LarpakeServer.Data;
using LarpakeServer.Data.Helpers;
using LarpakeServer.Data.PostgreSQL;
using LarpakeServer.Data.TypeHandlers;
using LarpakeServer.Identity;
using LarpakeServer.Identity.EntraId;
using LarpakeServer.Services;
using LarpakeServer.Services.Implementations;
using LarpakeServer.Services.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using Env = LarpakeServer.Helpers.Constants;

namespace LarpakeServer;

public static class ServiceInjections
{
    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                // Allow all
                builder.SetIsOriginAllowed(origin => true)
                       .AllowAnyMethod()
                       .AllowAnyHeader();

#if DEBUG
                // Add more permissions for development ports
                builder.WithOrigins([
                    "http://localhost:3000/",
                    "http://localhost:3001/",
                    "http://localhost:3002/",
                    "http://localhost:4000/",
                    "http://localhost:4001/",
                    "http://localhost:4002/"
                    ])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
#endif
            });
        });
    }

    public static void AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
          // Key options parsing from appsettings.json
        services.AddOptions<AttendanceKeyOptions>()
            .BindConfiguration(AttendanceKeyOptions.SectionName);



        // Conflict retry policy options parsing from appsettings.json
        services.AddOptions<ConflictRetryPolicyOptions>()
            .BindConfiguration(ConflictRetryPolicyOptions.SectionName);

        // Invite key options parsing from appsettings.json
        services.AddOptions<InviteKeyOptions>()
            .BindConfiguration(InviteKeyOptions.SectionName);

        // Luuppi integration options
        services.AddOptions<IntegrationOptions>()
            .BindConfiguration(IntegrationOptions.SectionName);


        // Options that read secrets from environment variables

        // Permissions
        PermissionsOptions permissionsOptions = new();
        

        services.AddOptions<PermissionsOptions>()
            .BindConfiguration(PermissionsOptions.SectionName);

        // Integration
        IntegrationOptions integrationOptions = Options.GetIntegrationOptions(configuration);
        services.AddSingleton<IOptions<IntegrationOptions>>(new OptionsContainer<IntegrationOptions>(integrationOptions));

        // Id
        LarpakeIdOptions idOptions = Options.GetLarpakeIdOptions(configuration);
        services.AddSingleton<IOptions<LarpakeIdOptions>>(new OptionsContainer<LarpakeIdOptions>(idOptions));

        // EntraId
        EntraIdOptions entraIdOptions = Options.GetEntraIdOptions(configuration);
        services.AddSingleton<IOptions<EntraIdOptions>>(new OptionsContainer<EntraIdOptions>(entraIdOptions));
    }

    public static void AddRateLimiters(this IServiceCollection services, IConfiguration configuration)
    {
        // Get configuration and validate
        RateLimitingOptions rateLimitingOptions = new() { Authentication = new(), General = new() };
        configuration.GetSection(RateLimitingOptions.SectionName).Bind(rateLimitingOptions);

        Guard.ThrowIfNegative(rateLimitingOptions.Authentication.PeriodSeconds);
        Guard.ThrowIfNegative(rateLimitingOptions.Authentication.MaxRequests);
        Guard.ThrowIfNegative(rateLimitingOptions.General.PeriodSeconds);
        Guard.ThrowIfNegative(rateLimitingOptions.General.MaxRequests);


        services.AddRateLimiter(config =>
            config.AddFixedWindowLimiter(RateLimitingOptions.GeneralPolicyName, options =>
            {
                var optionsSection = rateLimitingOptions.General;

                options.PermitLimit = optionsSection.MaxRequests;
                options.Window = TimeSpan.FromSeconds(optionsSection.PeriodSeconds);
                options.QueueLimit = 0;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            }));

        services.AddRateLimiter(config =>
            config.AddFixedWindowLimiter(RateLimitingOptions.AuthPolicyName, options =>
            {
                var optionsSection = rateLimitingOptions.Authentication;

                options.PermitLimit = optionsSection.MaxRequests;
                options.Window = TimeSpan.FromSeconds(optionsSection.PeriodSeconds);
                options.QueueLimit = 0;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            }));
    }

    public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        //This prevents 'sub' claim to be mapped incorrectly
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        // Injections
        services.AddSingleton<EntraTokenReader>();

        //Larpake id scheme is used by default
        services.AddAuthentication(Env.Auth.LarpakeIdScheme)
            .AddJwt(Env.Auth.LarpakeIdScheme, configuration)
            .AddEntraAuthenticationService(Env.Auth.EntraIdScheme, configuration);
    }




    public static void AddPostgresDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = 
            Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? configuration.GetConnectionString("PostgreSQL")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("No connection string found.");

        services.AddSingleton(new NpgsqlConnectionString(connectionString));
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler());
        SqlMapper.SetTypeMap(typeof(Models.DatabaseModels.Attendance), 
            new ColumnAttributeTypeMapper<Models.DatabaseModels.Attendance>());
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddSingleton<IOrganizationEventDatabase, OrganizationEventDatabase>();
        services.AddSingleton<IUserDatabase, UserDatabase>();
        services.AddSingleton<IGroupDatabase, GroupDatabase>();
        services.AddSingleton<IAttendanceDatabase, AttendanceDatabase>();
        services.AddSingleton<ISignatureDatabase, SignatureDatabase>();
        services.AddSingleton<IRefreshTokenDatabase, RefreshTokenDatabase>();
        services.AddSingleton<ILarpakeDatabase, LarpakeDatabase>();
        services.AddSingleton<ILarpakeTaskDatabase, LarpakeTaskDatabase>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<IExternalDataDbService, ExternalDataDbService>();
    }

    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationOptions(configuration);

        services.AddSingleton<CompletionMessageService>();
        services.AddSingleton(new ClientPoolConfiguration
        {
            MaxSize = configuration.GetValue<int>("SSE:InMemoryClientPoolSize")
        });
        services.AddSingleton<IClientPool, InMemoryClientPool>();
        services.AddSingleton<TokenService>();
        services.AddSingleton<IClaimsReader, TokenService>();
        services.AddSingleton<AttendanceKeyService>();
        services.AddSingleton<InviteKeyService>();
        services.AddSingleton<IExternalIntegrationService, LuuppiIntegrationService>();
        services.AddHttpClients(configuration);
    }


    private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        IntegrationOptions options = Options.GetIntegrationOptions(configuration);
        services.AddHttpClient(Env.HttpClients.IntegrationClient, client =>
        {
            client.BaseAddress = new Uri(options.BasePath);
            client.DefaultRequestHeaders.Add("Authorization", options.ApiKey);
            client.Timeout = new TimeSpan(0, 0, 20);
        });
        return services;
    }


    private static AuthenticationBuilder AddJwt(this AuthenticationBuilder builder, string authenticationScheme, IConfiguration configuration)
    {
        LarpakeIdOptions idOptions = Options.GetLarpakeIdOptions(configuration);
        Guard.ThrowIfNull(idOptions.SecretBytes);
        
        return builder.AddJwtBearer(authenticationScheme, options =>
        {
            options.TokenValidationParameters = new()
            {
                IssuerSigningKey = new SymmetricSecurityKey(idOptions.SecretBytes),
                ValidIssuer = idOptions.Issuer,
                ValidAudience = idOptions.Audience,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
            };
        });
    }

    public static AuthenticationBuilder AddEntraAuthenticationService(
       this AuthenticationBuilder builder, string authenticationScheme, IConfiguration configuration)
    {
        EntraIdOptions options = Options.GetEntraIdOptions(configuration);

        builder.AddMicrosoftIdentityWebApi(_ => { }, config =>
        {
            config.Instance = options.Instance!;
            config.TenantId = options.TenantId;
            config.ClientId = options.ClientId;
        }, authenticationScheme);
        return builder;
    }



    private static class Options
    {
        internal static LarpakeIdOptions GetLarpakeIdOptions(IConfiguration configuration)
        {
            LarpakeIdOptions options = new();
            configuration.GetSection(LarpakeIdOptions.SectionName).Bind(options);

            // Override with possible environment variable
            options.OverrideFromEnvironment();
            return options;
        }


        internal static IntegrationOptions GetIntegrationOptions(IConfiguration configuration)
        {
            IntegrationOptions options = new();
            configuration.GetSection(IntegrationOptions.SectionName).Bind(options);

            // Override with possible environment variable
            options.OverrideFromEnvironment();
            
            Debug.WriteLineIf(options.ApiKey is null, "Integration api key is null");
            return options;
        }

        internal static EntraIdOptions GetEntraIdOptions(IConfiguration configuration)
        {
            EntraIdOptions options = new();
            configuration.GetSection(EntraIdOptions.SectionName).Bind(options);

            // Override with possible environment variable
            string? tenantId = Environment.GetEnvironmentVariable(Env.Environment.EntraTenantId);
            if (tenantId is not null)
            {
                options.TenantId = tenantId;
            }

            // Override with possible environment variable
            string? clientId = Environment.GetEnvironmentVariable(Env.Environment.EntraClientId);
            if (clientId is not null)
            {
                options.ClientId = clientId;
            }
            return options;
        }

        internal static PermissionsOptions GetPermissionsOptions(IConfiguration configuration)
        {
            PermissionsOptions options = new();
            configuration.GetSection(PermissionsOptions.SectionName).Bind(options);

            string? sudoUsers = Environment.GetEnvironmentVariable(Env.Environment.EntraSudoUsers);
            if (sudoUsers is not null)
            {
                options.AddSudoUsersFromString(sudoUsers.AsSpan());
            }
            return options;
        }
    }


    


}
