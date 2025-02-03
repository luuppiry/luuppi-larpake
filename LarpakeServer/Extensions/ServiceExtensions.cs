using LarpakeServer.Data;
using LarpakeServer.Data.TypeHandlers;
using LarpakeServer.Identity;
using LarpakeServer.Services;
using LarpakeServer.Services.Implementations;
using LarpakeServer.Services.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using Postgres = LarpakeServer.Data.PostgreSQL;

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


    public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        // This prevents 'sub' claim to be mapped incorrectly
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
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




    public static void AddPostgresDatabases(this IServiceCollection services, IConfiguration configuration)
    {

        string connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("No connection string found.");

        services.AddSingleton(new NpgsqlConnectionString(connectionString));
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
        services.AddSingleton<IStatisticsService, Postgres.StatisticsService>();
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




        // Key options parsing from appsettings.json
        services.AddOptions<AttendanceKeyOptions>()
            .BindConfiguration(AttendanceKeyOptions.SectionName);

        // Permissions options parsing from appsettings.json
        services.AddOptions<PermissionsOptions>()
            .BindConfiguration(PermissionsOptions.SectionName);


    }

}
