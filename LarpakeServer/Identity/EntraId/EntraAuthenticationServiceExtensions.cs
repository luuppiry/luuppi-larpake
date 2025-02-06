using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace LarpakeServer.Identity.EntraId;

public static class EntraAuthenticationServiceExtensions
{
    public static IServiceCollection AddEntraAuthenticationService(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(
                config.GetSection(EntraIdOptions.SectionName));

        return services;
    }
}
    