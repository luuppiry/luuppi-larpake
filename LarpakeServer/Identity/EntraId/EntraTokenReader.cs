using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace LarpakeServer.Identity.EntraId;

public class EntraTokenReader
{
    public Guid? GetUserId(ClaimsPrincipal principal)
    {
        Guard.ThrowIfNull(principal);

        Claim? idClaim = principal.FindFirst("oid");
        if (idClaim is null)
        {
            return null;
        }
        _ = Guid.TryParse(idClaim.Value, out Guid value);
        return value;
    }
    
    public string? GetUsername(ClaimsPrincipal principal)
    {
        Guard.ThrowIfNull(principal);

        Claim? emailClaim = principal.FindFirst(JwtRegisteredClaimNames.PreferredUsername);
        return emailClaim?.Value;
    }
}
