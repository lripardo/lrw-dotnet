using System.Collections.Concurrent;
using LRW.Core.Date;

namespace LRW.Core.Cache;

public class MemoryCache<T>(IDateTimeResolver dateTimeResolver) : ICache<T>
{
    private readonly ConcurrentDictionary<string, MemoryCacheItem<T>> _dictionary = new();

    public T? Get(string key)
    {
        var item = _dictionary.GetValueOrDefault(key);

        if (item == null) return default;

        if (item.ExpiresAt == null || dateTimeResolver.Now <= item.ExpiresAt) return item.Value;

        Del(key);

        return default;
    }

    public void Set(string key, T value, TimeSpan duration)
    {
        _dictionary[key] = new MemoryCacheItem<T>(value, duration.Ticks != 0 ? dateTimeResolver.Now.Add(duration) : null);
    }

    public void Del(string key)
    {
        if (!_dictionary.TryRemove(key, out _))
        {
            throw new InvalidOperationException($"Cannot remove item {key}");
        }
    }

    public Task<T?> GetAsync(string key)
    {
        return Task.FromResult(Get(key));
    }

    public Task SetAsync(string key, T value, TimeSpan duration)
    {
        Set(key, value, duration);

        return Task.CompletedTask;
    }

    public Task DelAsync(string key)
    {
        Del(key);

        return Task.CompletedTask;
    }
}
