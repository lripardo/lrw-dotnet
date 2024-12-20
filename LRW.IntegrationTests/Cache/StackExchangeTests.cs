﻿using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using LRW.Cache.StackExchange;
using LRW.Core.Configuration;
using StackExchange.Redis;

namespace LRW.IntegrationTests.Cache;

public sealed class RedisFixture : IAsyncLifetime
{
    public RedisConnection Connection { get; set; } = null!;

    private readonly IContainer _container = new ContainerBuilder()
        .WithImage("redis:alpine3.15")
        .WithPortBinding(6379)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var configSource = new DictionaryConfigSource() { { "REDIS_HOST", "127.0.0.1" }, { "REDIS_PORT", "6379" } };

        Connection = new RedisConnection(new KeyedConfig(configSource));
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }
}

public sealed class StackExchangeTests(RedisFixture redis) : IClassFixture<RedisFixture>
{
    #region RedisCache

    private class TestRedisDatabase(int database, IKeyedConfigRepository<ConnectionMultiplexer> repository) : RedisCache<dynamic>(database, repository);

    [Fact]
    public async Task SetAsync_MustStoreCorrectValueOnRedisServer()
    {
        //Arrange
        var proofObject = new { IdTest = 1, NameTest = "Test1", IsEnabled = true, RateTest = 5.0, ArrayTest = new[] { "TEST1", "TEST2" } };
        var cache = new TestRedisDatabase(1, redis.Connection);

        //Act
        await cache.SetAsync("TEST1", proofObject, TimeSpan.Zero);
        var storedValue = redis.Connection.Instance.GetDatabase(1).StringGet("TEST1");

        //Assert
        Assert.Equal("{\"id_test\":1,\"name_test\":\"Test1\",\"is_enabled\":true,\"rate_test\":5,\"array_test\":[\"TEST1\",\"TEST2\"]}", storedValue);
    }

    [Fact]
    public void Set_MustStoreCorrectValueOnRedisServer()
    {
        //Arrange
        var proofObject = new { IdTest = 2, NameTest = "Test2", IsEnabled = false, RateTest = 10.0, ArrayTest = new[] { "TEST3", "TEST4" } };
        var cache = new TestRedisDatabase(2, redis.Connection);

        //Act
        cache.Set("TEST2", proofObject, TimeSpan.Zero);
        var storedValue = redis.Connection.Instance.GetDatabase(2).StringGet("TEST2");

        //Assert
        Assert.Equal("{\"id_test\":2,\"name_test\":\"Test2\",\"is_enabled\":false,\"rate_test\":10,\"array_test\":[\"TEST3\",\"TEST4\"]}", storedValue);
    }

    #endregion
}
