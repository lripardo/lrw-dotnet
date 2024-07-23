using FluentValidation;
using LRW.Core.Configuration;
using StackExchange.Redis;

namespace LRW.Cache.StackExchange;

public class RedisConnection(IKeyedConfig configuration) : SingletonKeyedConfigRepository<ConnectionMultiplexer>(configuration)
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
            RuleFor(x => x.Int).GreaterThanOrEqualTo(0);
        }
    }

    public class SecureConnectionKey() : Key("REDIS_SSL", "false");

    protected override ConnectionMultiplexer Make(IKeyedConfig c) => ConnectionMultiplexer.Connect(new ConfigurationOptions
    {
        EndPoints = { { c[new HostKey()].String, c[new PortKey()].Int } },
        Password = c[new PasswordKey()].String,
        KeepAlive = c[new KeepAliveKey()].Int,
        Ssl = c[new SecureConnectionKey()].Bool
        // ...
    });
}
