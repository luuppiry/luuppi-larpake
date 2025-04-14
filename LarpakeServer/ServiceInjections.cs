using LarpakeServer.Data;
using LarpakeServer.Data.Helpers;
using LarpakeServer.Data.PostgreSQL;
using LarpakeServer.Data.TypeHandlers;
using LarpakeServer.Identity;
using LarpakeServer.Identity.EntraId;
using LarpakeServer.Middleware;
using LarpakeServer.Services;
using LarpakeServer.Services.Implementations;
using LarpakeServer.Services.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
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
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });
    }

    public static void AddApplicationOptions(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger<DITypeMarker> logger)
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

        // Options that read secrets from environment variables

        // Integration
        IntegrationOptions integrationOptions = Options.GetIntegrationOptions(configuration, logger);
        services.AddSingleton<IOptions<IntegrationOptions>>(new OptionsContainer<IntegrationOptions>(integrationOptions));

        // Id
        LarpakeIdOptions idOptions = Options.GetLarpakeIdOptions(configuration, logger);
        services.AddSingleton<IOptions<LarpakeIdOptions>>(new OptionsContainer<LarpakeIdOptions>(idOptions));

        idOptions.LogValues(logger);

        // EntraId
        EntraIdOptions entraIdOptions = Options.GetEntraIdOptions(configuration, logger);
        services.AddSingleton<IOptions<EntraIdOptions>>(new OptionsContainer<EntraIdOptions>(entraIdOptions));

        PermissionsOptions permissionsOptions = Options.GetPermissionsOptions(configuration, logger);
        services.AddSingleton<IOptions<PermissionsOptions>>(new OptionsContainer<PermissionsOptions>(permissionsOptions));
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

    public static void AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger<DITypeMarker> logger)
    {
        //This prevents 'sub' claim to be mapped incorrectly
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        // Injections
        services.AddSingleton<EntraTokenReader>();

        //Larpake id scheme is used by default
        services.AddAuthentication(Env.Auth.LarpakeIdScheme)
            .AddJwt(Env.Auth.LarpakeIdScheme, configuration, logger)
            .AddEntraAuthenticationService(Env.Auth.EntraIdScheme, configuration, logger);
    }




    public static void AddPostgresDatabases(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger<DITypeMarker> logger)
    {
        string? connectionString = configuration[Env.Environment.PostgresConnectionString];
        if (connectionString is not null)
        {
            logger.LogInformation("Using Postgres connection string from environment variables.");
        }

        connectionString
            ??= configuration.GetConnectionString("PostgreSQL")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Postgres connection string is null.");

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

    public static void AddServices(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger<DITypeMarker> logger)
    {
        services.AddApplicationOptions(configuration, logger);

        services.AddSingleton<UserService>();
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
        services.AddHttpClients(configuration, logger);
    }


    private static IServiceCollection AddHttpClients(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger<DITypeMarker> logger)
    {
        IntegrationOptions options = Options.GetIntegrationOptions(configuration, logger);
        services.AddHttpClient(Env.HttpClients.IntegrationClient, client =>
        {
            client.BaseAddress = new Uri(options.BasePath);
            client.DefaultRequestHeaders.Add("Authorization", options.ApiKey);
            client.Timeout = new TimeSpan(0, 0, 20);
        });
        return services;
    }


    private static AuthenticationBuilder AddJwt(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        IConfiguration configuration,
        ILogger<DITypeMarker> logger)
    {
        LarpakeIdOptions idOptions = Options.GetLarpakeIdOptions(configuration, logger);
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
       this AuthenticationBuilder builder,
       string authenticationScheme,
       IConfiguration configuration,
       ILogger<DITypeMarker> logger)
    {
        EntraIdOptions options = Options.GetEntraIdOptions(configuration, logger);

        builder.AddMicrosoftIdentityWebApi(_ => { }, config =>
        {
            config.Instance = options.Instance!;
            config.TenantId = options.TenantId;
            config.ClientId = options.ClientId;
        }, authenticationScheme);
        return builder;
    }


    public static WebApplication AddStaticFrontend(
        this WebApplication app, ILogger<DITypeMarker> logger)
    {
        // Production add frontend page serving
        logger.LogInformation("Adding frontend serving.");
        app.AddFrontendServing(logger);
        try
        {
            if (Directory.Exists("wwwroot"))
            {
                logger.LogInformation("wwwroot exists");
            }
            else
            {
                logger.LogInformation("wwwroot does not exist");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error checking wwwroot directory");
        }
        return app;
    }


    private static class Options
    {
        internal static LarpakeIdOptions GetLarpakeIdOptions(IConfiguration configuration, ILogger<DITypeMarker> logger)
        {
            LarpakeIdOptions options = new();
            configuration.GetSection(LarpakeIdOptions.SectionName).Bind(options);

            // Override with possible environment variable
            string? secret = configuration[Env.Environment.LarpakeIdSecret]; 
            if (secret is not null)
            {
                logger.LogInformation("Overriding LarpakeId JWT secret from environment variables.");
                options.SecretKey = secret;
            }

            string? issuer = configuration[Env.Environment.LarpakeIdIssuer];
            if (issuer is not null)
            {
                logger.LogInformation("Overriding LarpakeId JWT issuer from environment variables.");
                options.Issuer = issuer;
            }

            string? audience = configuration[Env.Environment.LarpakeIdAudience];
            if (audience is not null)
            {
                logger.LogInformation("Overriding LarpakeId JWT audience from environment variables.");
                options.Audience = audience;
            }


            logger.IfNull(options.SecretKey).LogInformation("LarpakeId secret key is null.");
            logger.IfNull(options.Issuer).LogInformation("LarpakeId issuer is null.");
            logger.IfNull(options.Audience).LogInformation("LarpakeId audience is null.");


            return options;
        }


        internal static IntegrationOptions GetIntegrationOptions(IConfiguration configuration, ILogger<DITypeMarker> logger)
        {
            IntegrationOptions options = new();
            configuration.GetSection(IntegrationOptions.SectionName).Bind(options);

            // Override with possible environment variable
            string? apiKey = configuration[Env.Environment.LuuppiApiKey];
            if (apiKey is not null)
            {
                logger.LogInformation("Overriding integration api key with environment variables.");
                options.ApiKey = apiKey;
            }

            logger.IfNull(options.ApiKey).LogInformation("Integration api key is null.");
            return options;
        }

        internal static EntraIdOptions GetEntraIdOptions(IConfiguration configuration, ILogger<DITypeMarker> logger)
        {
            EntraIdOptions options = new();
            configuration.GetSection(EntraIdOptions.SectionName).Bind(options);

            // Override with possible environment variable
            string? tenantId = configuration[Env.Environment.EntraTenantId];
            if (tenantId is not null)
            {
                logger.LogInformation("Overriding EntraId tenant id with environment variable.");
                options.TenantId = tenantId;
            }

            // Override with possible environment variable
            string? clientId = configuration[Env.Environment.EntraClientId];
            if (clientId is not null)
            {
                logger.LogInformation("Overriding EntraId client id with environment variable.");
                options.ClientId = clientId;
            }


            logger.IfNull(options.ClientId).LogInformation("Entra client id is null.");
            logger.IfNull(options.TenantId).LogInformation("Entra tedant id is null.");

            return options;
        }

        internal static PermissionsOptions GetPermissionsOptions(IConfiguration configuration, ILogger<DITypeMarker> logger)
        {
            PermissionsOptions options = new();
            configuration.GetSection(PermissionsOptions.SectionName).Bind(options);

            string? sudoUsers = configuration[Env.Environment.EntraSudoUsers];
            if (sudoUsers is not null)
            {
                logger.LogInformation("Overriding Entra sudo users with environment variable.");
                options.AddSudoUsersFromString(sudoUsers.AsSpan());
            }

            logger.IfNull(options.EntraSudoModeUsers).LogInformation("Entra sudo users is null.");
            return options;
        }
    }





}
