using LarpakeServer.Identity;
using LarpakeServer.Models.GetDtos.Templates;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LarpakeServer.Extensions;

public static class HelperExtensions
{
    public static void SetNextPaginationPage(this IPageable pageable, QueryOptions options)
    {
        if (pageable.ItemCount == options.PageSize)
        {
            pageable.NextPage = options.GetNextOffset();
        }
    }

    public static Guid ReadAuthorizedUserId(this IClaimsReader reader, HttpRequest request)
    {
        return reader.GetUserId(request.HttpContext.User)
            ?? throw new InvalidOperationException("User not found in claims, is user authenticated correctly?");
    }

    public static Permissions ReadAuthorizedUserPermissions(this IClaimsReader reader, HttpRequest request)
    {
        return reader.GetUserPermissions(request.HttpContext.User)
            ?? throw new InvalidOperationException("User not found in claims, is user authenticated correctly?");
    }

    public static int? ReadAuthorizedUserStartYear(this IClaimsReader reader, HttpRequest request)
    {
        return reader.GetUserStartYear(request.HttpContext.User);
    }
}
