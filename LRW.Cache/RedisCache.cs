using FluentValidation;
using LRW.Core.Cache;
using LRW.Core.Configuration;
using LRW.Core.Helpers;
using StackExchange.Redis;

namespace LRW.Cache;

public static class RedisConnection
{
    private static ConnectionMultiplexer? _connection;

    public static ConnectionMultiplexer GetConnection(ConfigurationOptions options)
    {
        return _connection ??= ConnectionMultiplexer.Connect(options);
    }
}

public abstract class RedisCache<T> : ICache<T>
{
    public class HostKey : Key
    {
        public HostKey() : base("REDIS_HOST", "127.0.0.1")
        {
            RuleFor(x => x.String).NotEmpty();
        }
    }

    public class PortKey : Key
    {
        public PortKey() : base("REDIS_PORT", "6379")
        {
            RuleFor(x => x.Int).GreaterThanOrEqualTo(1).LessThanOrEqualTo(65535);
        }
    }

    public class PasswordKey : Key
    {
        public PasswordKey() : base("REDIS_PASSWORD", "redis")
        {
            RuleFor(x => x.String).NotEmpty();
        }
    }

    public class KeepAliveKey : Key
    {
        public KeepAliveKey() : base("REDIS_KEEP_ALIVE", "180", ["Time in seconds"])
        {
            RuleFor(x => x.Int);
        }
    }

    public class SecureConnectionKey() : Key("REDIS_SSL", "false");

    private readonly ConnectionMultiplexer _connection;
    private readonly int _database;

    protected RedisCache(IKeyConfiguration configuration, int database)
    {
        var host = configuration[new HostKey()].String;
        var port = configuration[new PortKey()].Int;
        var password = configuration[new PasswordKey()].String;
        var secureConnection = configuration[new SecureConnectionKey()].Bool;
        var keepAlive = configuration[new KeepAliveKey()].Int;

        var options = new ConfigurationOptions
        {
            EndPoints = { { host, port } },
            KeepAlive = keepAlive,
            Password = password,
            Ssl = secureConnection
        };

        _connection = RedisConnection.GetConnection(options);
        _database = database;
    }

    public T? Get(string key)
    {
        var data = _connection.GetDatabase(_database).StringGet(key);

        return data.HasValue ? JsonHelper.Deserialize<T?>(data.ToString()) : default;
    }

    public void Set(string key, T value, TimeSpan duration)
    {
        var serialized = JsonHelper.Serialize(value);

        _connection.GetDatabase(_database).StringSet(key, serialized, duration);
    }

    public void Del(string key)
    {
        _connection.GetDatabase(_database).KeyDelete(key);
    }

    public async Task<T?> GetAsync(string key)
    {
        var data = await _connection.GetDatabase(_database).StringGetAsync(key);

        return data.HasValue ? JsonHelper.Deserialize<T?>(data.ToString()) : default;
    }

    public Task SetAsync(string key, T value, TimeSpan duration)
    {
        var serialized = JsonHelper.Serialize(value);

        return _connection.GetDatabase(_database).StringSetAsync(key, serialized, duration);
    }

    public Task DelAsync(string key)
    {
        return _connection.GetDatabase(_database).KeyDeleteAsync(key);
    }
}
