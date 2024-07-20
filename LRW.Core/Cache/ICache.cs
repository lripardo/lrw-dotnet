namespace LRW.Core.Cache;

public interface ICache<T>
{
    T? Get(string key);
    void Set(string key, T value, TimeSpan duration);
    void Del(string key);

    Task<T?> GetAsync(string key);
    Task SetAsync(string key, T value, TimeSpan duration);
    Task DelAsync(string key);
}
