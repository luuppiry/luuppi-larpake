using System.Security.Claims;

namespace LarpakeServer.Identity;

public interface IClaimsReader
{
    Guid? GetUserId(ClaimsPrincipal principal);
}