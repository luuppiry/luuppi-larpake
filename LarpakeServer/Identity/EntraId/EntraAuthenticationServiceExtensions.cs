using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;

namespace LarpakeServer.Identity.EntraId;

public static class EntraAuthenticationServiceExtensions
{
    public static AuthenticationBuilder AddEntraAuthenticationService(
        this AuthenticationBuilder builder, string authenticationScheme, IConfiguration configuration)
    {
        builder.AddMicrosoftIdentityWebApi(configuration.GetSection(EntraIdOptions.SectionName), authenticationScheme);
        return builder;
    }
}
    