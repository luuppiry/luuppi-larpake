using System.Security.Claims;

namespace LarpakeServer.Identity;

public interface IClaimsReader
{
    Guid? GetUserId(ClaimsPrincipal principal);

    Permissions? GetUserPermissions(ClaimsPrincipal principal);

    DateTime? GetTokenIssuedAt(ClaimsPrincipal principal);
    
    int? GetUserStartYear(ClaimsPrincipal principal);
}