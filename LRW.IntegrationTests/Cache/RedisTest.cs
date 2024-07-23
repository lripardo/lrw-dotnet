using DotNet.Testcontainers.Builders;
using FakeItEasy;
using LRW.Cache.StackExchange;
using LRW.Core.Configuration;
using StackExchange.Redis;

namespace LRW.IntegrationTests.Cache;

public class RedisTest
{
    private const int DatabaseNumber = 5;
    private static readonly TaskFactory TaskFactory = new(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

    private readonly ConnectionMultiplexer _connection;

    public RedisTest()
    {
        TaskFactory.StartNew(LoadAsync).Unwrap().ConfigureAwait(false).GetAwaiter().GetResult();

        _connection = ConnectionMultiplexer.Connect(new ConfigurationOptions() { EndPoints = { { "127.0.0.1", 6379 } }, Protocol = RedisProtocol.Resp3 });
    }

    private static async Task LoadAsync()
    {
        var container = new ContainerBuilder()
            .WithImage("redis:alpine3.15")
            .WithPortBinding(6379)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

        await container.StartAsync().ConfigureAwait(false);
    }

    private class MyTestJson
    {
        public int IdTest { get; set; }
        public string? NameTest { get; set; }
        public bool IsEnabled { get; set; }
        public double RateTest { get; set; }
        public IEnumerable<string>? ArrayTest { get; set; }
    }

    private class TestRedisDatabase(IKeyedConfigRepository<ConnectionMultiplexer> source) : RedisCache<MyTestJson>(DatabaseNumber, source);

    [Fact]
    public async Task SetAsync_RedisCache_MustStoreCorrectValueOnRedisServer()
    {
        //Arrange
        var json1 = new MyTestJson()
        {
            IdTest = 1,
            NameTest = "Test1",
            IsEnabled = true,
            RateTest = 5.0,
            ArrayTest = ["TEST1", "TEST2"]
        };

        const string expected = "{\"id_test\":1,\"name_test\":\"Test1\",\"is_enabled\":true,\"rate_test\":5,\"array_test\":[\"TEST1\",\"TEST2\"]}";

        var source = A.Fake<IKeyedConfigRepository<ConnectionMultiplexer>>();
        var redis = new TestRedisDatabase(source);

        A.CallTo(() => source.Instance).Returns(_connection);

        //Act
        await redis.SetAsync("TEST1", json1, TimeSpan.Zero);

        var storedValue = _connection.GetDatabase(DatabaseNumber).StringGet("TEST1");

        //Assert
        Assert.Equal(expected, storedValue);
        A.CallTo(() => source.Instance).MustHaveHappenedOnceExactly();
    }
}
