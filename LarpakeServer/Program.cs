using LarpakeServer;
using LarpakeServer.Identity;
using LarpakeServer.Services.Implementations;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Order matters with hosted services (first is executed first)
builder.Services.AddHostedService<PermissionsStartupService>();

// Add services to the container.
ConfigurationManager configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<LarpakeIdBearerSecuritySchemeTransformer>();
});
builder.Services.AddAuthenticationServices(configuration);
builder.Services.AddAuthorization();
builder.Services.AddServices(configuration);
builder.Services.AddPostgresDatabases(configuration);
builder.Services.ConfigureCors();
builder.Services.AddRateLimiters(configuration);

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle(configuration["Scalar-OpenApi:Title"]!);
    });
}

// TODO: Add exception handling middleware

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
