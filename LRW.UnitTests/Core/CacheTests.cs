using System.Collections.Concurrent;
using FakeItEasy;
using LRW.Core.Cache;
using LRW.Core.Date;
using System.Reflection;

namespace LRW.UnitTests.Core;

public class CacheTests
{
    #region MemoryCache

    [Fact]
    public void Set_MustStoreCorrectObjectWithCorrectExpiresAt()
    {
        //Arrange
        var dictionary = new ConcurrentDictionary<string, MemoryCacheItem<string>>();

        var dateResolver = A.Fake<IDateTimeResolver>();
        var cache = new MemoryCache<string>(dateResolver);

        A.CallTo(() => dateResolver.Now).Returns(DateTime.Now);

        //Act
        cache.Set("TEST1", "MY VALUE", TimeSpan.Zero);
        
        typeof(MemoryCache<string>).GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(cache, dictionary);

        //Assert
        Assert.NotNull(dictionary);

        var result = dictionary["TEST1"];

        Assert.Equal("MY VALUE", result.Value);
        Assert.Null(result.ExpiresAt);

        A.CallTo(() => dateResolver.Now).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetAsync_MustStoreCorrectObjectWithCorrectExpiresAt()
    {
        //Arrange
        var dictionary = new ConcurrentDictionary<string, MemoryCacheItem<string>>();

        var dateResolver = A.Fake<IDateTimeResolver>();
        var cache = new MemoryCache<string>(dateResolver);

        A.CallTo(() => dateResolver.Now).Returns(DateTime.Now);

        //Act
        await cache.SetAsync("TEST1", "MY VALUE", TimeSpan.Zero);

        typeof(MemoryCache<string>).GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(cache, dictionary);

        //Assert
        Assert.NotNull(dictionary);

        var result = dictionary["TEST1"];

        Assert.Equal("MY VALUE", result.Value);
        Assert.Null(result.ExpiresAt);

        A.CallTo(() => dateResolver.Now).MustNotHaveHappened();
    }

    [Fact]
    public void Get_MustReturnsStoredObject()
    {
        //Arrange
        var dictionary = new ConcurrentDictionary<string, MemoryCacheItem<string>> { ["TEST2"] = new("MY VALUE 2") };

        var dateResolver = A.Fake<IDateTimeResolver>();
        var cache = new MemoryCache<string>(dateResolver);

        typeof(MemoryCache<string>).GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(cache, dictionary);

        //Act
        var result = cache.Get("TEST2");

        //Assert
        Assert.Equal("MY VALUE 2", result);
    }
    
    [Fact]
    public async Task GetAsync_MustReturnsStoredObject()
    {
        //Arrange
        var dictionary = new ConcurrentDictionary<string, MemoryCacheItem<string>> { ["TEST2"] = new("MY VALUE 2") };

        var dateResolver = A.Fake<IDateTimeResolver>();
        var cache = new MemoryCache<string>(dateResolver);

        typeof(MemoryCache<string>).GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(cache, dictionary);

        //Act
        var result = await cache.GetAsync("TEST2");

        //Assert
        Assert.Equal("MY VALUE 2", result);
    }


    [Fact]
    public void Del_MustRemoveObjectFromDictionary()
    {
        //Arrange
        var dictionary = new ConcurrentDictionary<string, MemoryCacheItem<string>> { ["TEST3"] = new("MY VALUE 3") };

        var dateResolver = A.Fake<IDateTimeResolver>();
        var cache = new MemoryCache<string>(dateResolver);

        typeof(MemoryCache<string>).GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(cache, dictionary);

        //Act
        cache.Del("TEST3");

        //Assert
        Assert.Throws<KeyNotFoundException>(() => dictionary["TEST3"]);
    }

    [Fact]
    public async Task DelAsync_MustRemoveObjectFromDictionary()
    {
        //Arrange
        var dictionary = new ConcurrentDictionary<string, MemoryCacheItem<string>> { ["TEST3"] = new("MY VALUE 3") };

        var dateResolver = A.Fake<IDateTimeResolver>();
        var cache = new MemoryCache<string>(dateResolver);

        typeof(MemoryCache<string>).GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(cache, dictionary);

        //Act
        await cache.DelAsync("TEST3");

        //Assert
        Assert.Throws<KeyNotFoundException>(() => dictionary["TEST3"]);
    }

    #endregion
}
