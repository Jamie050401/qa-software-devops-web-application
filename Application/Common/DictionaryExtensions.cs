namespace Application.Common;

using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static void ForceAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        dictionary.Remove(key);
        dictionary.Add(key, value);
    }
}