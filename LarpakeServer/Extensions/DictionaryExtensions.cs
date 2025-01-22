using System.Runtime.InteropServices;

namespace LarpakeServer.Extensions;

public static class DictionaryExtensions
{
    public static TValue? GetOrAdd<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary, 
        TKey key, 
        TValue value)
        where TKey : notnull
    {
        ref var val = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exists);
        if (exists)
        {
            return val;
        }
        val = value;
        return value;
    }
}
