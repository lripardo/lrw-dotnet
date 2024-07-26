using System.Collections.Concurrent;
using FakeItEasy;
using LRW.Core.Cache;
using LRW.Core.Date;
using System.Reflection;

namespace LRW.UnitTests.Core;

public static class CacheTestsExtensions
{
    public static ConcurrentDictionary<string, MemoryCacheItem<string>> GetDictionary(this MemoryCache<string> cache)
    {
        var dictionary = typeof(MemoryCache<string>).GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance);

        return (ConcurrentDictionary<string, MemoryCacheItem<string>>)dictionary?.GetValue(cache)!;
    }
}

public class CacheTests
{
    #region MemoryCache

    [Fact]
    public void Set_MustStoreCorrectObject()
    {
        //Arrange
        var cache = new MemoryCache<string>(A.Fake<IDateTimeResolver>());

        //Act
        cache.Set("TEST1", "MY VALUE 1", TimeSpan.Zero);

        //Assert
        var result = cache.GetDictionary()["TEST1"];

        Assert.Equal("MY VALUE 1", result.Value);
    }

    [Fact]
    public async Task SetAsync_MustStoreCorrectObject()
    {
        //Arrange
        var cache = new MemoryCache<string>(A.Fake<IDateTimeResolver>());

        //Act
        await cache.SetAsync("TEST1", "MY VALUE 1", TimeSpan.Zero);

        //Assert
        var result = cache.GetDictionary()["TEST1"];

        Assert.Equal("MY VALUE 1", result.Value);
    }

    [Fact]
    public void Get_MustReturnsStoredObject()
    {
        //Arrange
        var cache = new MemoryCache<string>(A.Fake<IDateTimeResolver>());
        cache.GetDictionary()["TEST2"] = new MemoryCacheItem<string>("MY VALUE 2");

        //Act
        var result = cache.Get("TEST2");

        //Assert
        Assert.Equal("MY VALUE 2", result);
    }

    [Fact]
    public async Task GetAsync_MustReturnsStoredObject()
    {
        //Arrange
        var cache = new MemoryCache<string>(A.Fake<IDateTimeResolver>());
        cache.GetDictionary()["TEST2"] = new MemoryCacheItem<string>("MY VALUE 2");

        //Act
        var result = await cache.GetAsync("TEST2");

        //Assert
        Assert.Equal("MY VALUE 2", result);
    }


    [Fact]
    public void Del_MustRemoveObjectFromDictionary()
    {
        //Arrange
        var cache = new MemoryCache<string>(A.Fake<IDateTimeResolver>());
        var dictionary = cache.GetDictionary();

        dictionary["TEST3"] = new MemoryCacheItem<string>("MY VALUE 3");

        //Act
        cache.Del("TEST3");

        //Assert
        Assert.Throws<KeyNotFoundException>(() => dictionary["TEST3"]);
    }

    [Fact]
    public async Task DelAsync_MustRemoveObjectFromDictionary()
    {
        //Arrange
        var cache = new MemoryCache<string>(A.Fake<IDateTimeResolver>());
        var dictionary = cache.GetDictionary();

        dictionary["TEST3"] = new MemoryCacheItem<string>("MY VALUE 3");

        //Act
        await cache.DelAsync("TEST3");

        //Assert
        Assert.Throws<KeyNotFoundException>(() => dictionary["TEST3"]);
    }

    [Fact]
    public void ObjectMustBeStoredWithInfiniteTimeWhenTimestampIsZero()
    {
        //Arrange
        var cache = new MemoryCache<string>(A.Fake<IDateTimeResolver>());

        //Act
        cache.Set("TEST4", "MY VALUE 4", TimeSpan.Zero);

        //Assert
        Assert.Equal("MY VALUE 4", cache.Get("TEST4"));
    }

    [Fact]
    public void ObjectExpiredMustBeDeletedFromDictionary()
    {
        //Arrange
        var dateResolver = A.Fake<IDateTimeResolver>();
        var cache = new MemoryCache<string>(dateResolver);
        cache.GetDictionary()["TEST5"] = new MemoryCacheItem<string>("MY VALUE 5", new DateTime(999));

        A.CallTo(() => dateResolver.Now).Returns(new DateTime(1000));

        //Act
        var result = cache.Get("TEST5");

        //Assert
        Assert.Null(result);
        Assert.Throws<KeyNotFoundException>(() => cache.GetDictionary()["TEST5"]);
    }

    #endregion
}
