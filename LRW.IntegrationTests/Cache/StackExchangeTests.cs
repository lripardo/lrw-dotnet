using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FakeItEasy;
using LRW.Cache.StackExchange;
using LRW.Core.Configuration;
using StackExchange.Redis;

namespace LRW.IntegrationTests.Cache;

public sealed class RedisFixture : IAsyncLifetime
{
    public ConnectionMultiplexer Connection { get; private set; } = null!;

    private readonly IContainer _container = new ContainerBuilder()
        .WithImage("redis:alpine3.15")
        .WithPortBinding(6379)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Connection = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions() { EndPoints = { { "127.0.0.1", 6379 } } });
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

        var repository = A.Fake<IKeyedConfigRepository<ConnectionMultiplexer>>();
        var cache = new TestRedisDatabase(1, repository);

        A.CallTo(() => repository.Instance).Returns(redis.Connection);

        //Act
        await cache.SetAsync("TEST1", proofObject, TimeSpan.Zero);

        var storedValue = redis.Connection.GetDatabase(1).StringGet("TEST1");

        //Assert
        Assert.Equal("{\"id_test\":1,\"name_test\":\"Test1\",\"is_enabled\":true,\"rate_test\":5,\"array_test\":[\"TEST1\",\"TEST2\"]}", storedValue);
        A.CallTo(() => repository.Instance).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Set_MustStoreCorrectValueOnRedisServer()
    {
        //Arrange
        var proofObject = new { IdTest = 2, NameTest = "Test2", IsEnabled = false, RateTest = 10.0, ArrayTest = new[] { "TEST3", "TEST4" } };

        var repository = A.Fake<IKeyedConfigRepository<ConnectionMultiplexer>>();
        var cache = new TestRedisDatabase(2, repository);

        A.CallTo(() => repository.Instance).Returns(redis.Connection);

        //Act
        cache.Set("TEST2", proofObject, TimeSpan.Zero);

        var storedValue = redis.Connection.GetDatabase(2).StringGet("TEST2");

        //Assert
        Assert.Equal("{\"id_test\":2,\"name_test\":\"Test2\",\"is_enabled\":false,\"rate_test\":10,\"array_test\":[\"TEST3\",\"TEST4\"]}", storedValue);
        A.CallTo(() => repository.Instance).MustHaveHappenedOnceExactly();
    }

    #endregion
}
