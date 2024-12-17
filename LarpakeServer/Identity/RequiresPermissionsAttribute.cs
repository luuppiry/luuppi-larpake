using LarpakeServer.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LarpakeServer.Identity;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiresPermissionsAttribute : Attribute, IAuthorizationFilter
{
    private readonly Permissions _requiredPermissions;

    public RequiresPermissionsAttribute(Permissions permissions)
    {
        _requiredPermissions = permissions;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        IEnumerable<Claim> claims = context.HttpContext.User.Claims;
        Claim? permissionsClaim = claims.LastOrDefault(x => x.Type is Constants.PermissionsFieldName);
        if (EnumExtensions.TryConvertPermissions(permissionsClaim?.Value, out var permissions))
        {
            if (permissions.Value.Has(_requiredPermissions))
            {
                return;
            }
        }
        context.Result = new ForbidResult();
    }
}
