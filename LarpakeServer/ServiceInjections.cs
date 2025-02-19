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

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;

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
        services.AddAuthentication(Constants.Auth.LarpakeIdScheme)
            .AddJwt(Constants.Auth.LarpakeIdScheme, configuration)
            .AddEntraAuthenticationService(Constants.Auth.EntraIdScheme, configuration);
    }




    public static void AddPostgresDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("No connection string found.");

        services.AddSingleton(new NpgsqlConnectionString(connectionString));
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler());
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
        services.AddSingleton<InviteKeyService>();

        // Key options parsing from appsettings.json
        services.AddOptions<AttendanceKeyOptions>()
            .BindConfiguration(AttendanceKeyOptions.SectionName);

        // Permissions options parsing from appsettings.json
        services.AddOptions<PermissionsOptions>()
            .BindConfiguration(PermissionsOptions.SectionName);

        // Conflict retry policy options parsing from appsettings.json
        services.AddOptions<ConflictRetryPolicyOptions>()
            .BindConfiguration(ConflictRetryPolicyOptions.SectionName);

        // Invite key options parsing from appsettings.json
        services.AddOptions<InviteKeyOptions>()
            .BindConfiguration(InviteKeyOptions.SectionName);
    }


    private static AuthenticationBuilder AddJwt(this AuthenticationBuilder builder, string authenticationScheme, IConfiguration configuration)
    {
        return builder.AddJwtBearer(authenticationScheme, options =>
        {
            options.TokenValidationParameters = new()
            {
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
            };
        });
    }
}
