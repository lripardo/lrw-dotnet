namespace LRW.Core.Cache;

public class MemoryCacheItem<T>(T value, DateTime? expiresAt = null)
{
    public T Value { get; } = value;
    public DateTime? ExpiresAt { get; } = expiresAt;
}
