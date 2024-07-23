using LRW.Core.Cache;
using LRW.Core.Configuration;
using LRW.Core.Helpers;
using StackExchange.Redis;

namespace LRW.Cache.StackExchange;

public abstract class RedisCache<T>(int database, IKeyedConfigRepository<ConnectionMultiplexer> connection) : ICache<T>
{
    public T? Get(string key)
    {
        var data = connection.Instance.GetDatabase(database).StringGet(key);

        return data.HasValue ? JsonHelper.Deserialize<T?>(data.ToString()) : default;
    }

    public void Set(string key, T value, TimeSpan duration)
    {
        var serialized = JsonHelper.Serialize(value);

        connection.Instance.GetDatabase(database).StringSet(key, serialized, duration);
    }

    public void Del(string key)
    {
        connection.Instance.GetDatabase(database).KeyDelete(key);
    }

    public async Task<T?> GetAsync(string key)
    {
        var data = await connection.Instance.GetDatabase(database).StringGetAsync(key);

        return data.HasValue ? JsonHelper.Deserialize<T?>(data.ToString()) : default;
    }

    public Task SetAsync(string key, T value, TimeSpan duration)
    {
        var serialized = JsonHelper.Serialize(value);

        return connection.Instance.GetDatabase(database).StringSetAsync(key, serialized, duration.Ticks == 0 ? null : duration);
    }

    public Task DelAsync(string key)
    {
        return connection.Instance.GetDatabase(database).KeyDeleteAsync(key);
    }
}
