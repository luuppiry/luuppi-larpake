using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Extensions;

public static class HelperExtensions
{
    public static void CalculateNextPageFrom(this IPageable pageable, QueryOptions options)
    {
        if (pageable.ItemCount == options.PageSize)
        {
            pageable.NextPage = options.GetNextOffset();
        }
    }
}
