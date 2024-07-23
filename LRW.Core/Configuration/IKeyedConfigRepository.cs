namespace LRW.Core.Configuration;

public interface IKeyedConfigRepository<out T>
{
    T Instance { get; }
}
