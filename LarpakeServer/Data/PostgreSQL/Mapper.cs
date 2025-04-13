using LarpakeServer.Extensions;
using LarpakeServer.Models.DatabaseModels.Metadata;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

internal static class Mapper 
{
    public static async Task<T?> QueryFirstOrDefaultLocalizedAsync<T, U>(
        this NpgsqlConnection connection, string sql, object? param = null, string splitOn = "Id") 
        where T : ILocalized<U>
    {
        T? result = default;
        await connection.QueryAsync<T, U, T>(sql, 
            (val, loc) => MapSingleLocalized(val, loc, ref result), 
            param, 
            splitOn: splitOn);

        return result;
    }

    public static async Task<Dictionary<long, T>> QueryLocalizedAsync<T, U>(
        this NpgsqlConnection connection, string sql, object? param = null, string splitOn = "Id") 
        where T : ILocalized<U>
    {
        Dictionary<long, T> result = [];
        await connection.QueryAsync<T, U, T>(sql, 
            (val, loc) => MapLocalized(val, loc, ref result), 
            param, 
            splitOn: splitOn);

        return result;
    }


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
