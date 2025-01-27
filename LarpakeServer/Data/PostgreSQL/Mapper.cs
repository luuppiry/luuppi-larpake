using LarpakeServer.Extensions;
using LarpakeServer.Models.DatabaseModels.Metadata;

namespace LarpakeServer.Data.PostgreSQL;

internal static class Mapper 
{
    public static T MapSingleLocalized<T, U>(T value, U localization, ref T? result) where T : ILocalized<U>
    {
        result ??= value;
        if (localization is not null)
        {
            result.TextData ??= [];
            result.TextData.Add(localization);
        }
        return result;
    }

    public static T MapLocalized<T, U>(T value, U localization, ref Dictionary<long, T> map) where T : ILocalized<U>
    {
        var result = map.GetOrAdd(value.Id, value)!;
        if (localization is not null)
        {
            result.TextData ??= [];
            result.TextData.Add(localization);
        }
        return result;
    }
}
