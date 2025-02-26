using LarpakeServer;
using LarpakeServer.Identity;
using LarpakeServer.Services.Implementations;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(Constants.Environment.EnvVariablePrefix);

using var loggerFactory = LoggerFactory.Create(config =>
{
    config.AddConsole();
#if DEBUG
    config.AddDebug();
#endif
});

ILogger<DITypeMarker> logger = loggerFactory.CreateLogger<DITypeMarker>();
logger.LogInformation("Added environment variables with prefix {prefix}", Constants.Environment.EnvVariablePrefix);



// Order matters with hosted services (first is executed first)
builder.Services.AddHostedService<PermissionsStartupService>();

// Add services to the container.
ConfigurationManager configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<LarpakeIdBearerSecuritySchemeTransformer>();
});
builder.Services.AddAuthenticationServices(configuration, logger);
builder.Services.AddAuthorization();
builder.Services.AddServices(configuration, logger);
builder.Services.AddPostgresDatabases(configuration, logger);
builder.Services.ConfigureCors();
builder.Services.AddRateLimiters(configuration);

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});


var app = builder.Build();


// Use openapi 
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle(configuration["Scalar-OpenApi:Title"]!);
});

// TODO: Add exception handling middleware

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
