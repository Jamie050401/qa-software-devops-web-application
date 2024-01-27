namespace Application.Data;

using Microsoft.Extensions.Caching.Memory;

public static class DatabaseManager
{
    public static Database Database { get; } = new();
    private static MemoryCache Cache { get; } = new(new MemoryCacheOptions());

    private static MemoryCacheEntryOptions CacheEntryOptions { get; } = new()
    {
        SlidingExpiration = TimeSpan.FromHours(1)
    };

    public static void DeleteFromCache(object? key)
    {
        if (key is not null) Cache.Remove(key);
    }

    public static void AddToCache(object key, object value)
    {
        Cache.Set(key, value, CacheEntryOptions);
    }

    public static object? GetFromCache(object? key)
    {
        if (key is null) return null;

        Cache.TryGetValue(key, out var value);
        return value;
    }
}
