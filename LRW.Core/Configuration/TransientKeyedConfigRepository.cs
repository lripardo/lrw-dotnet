namespace LRW.Core.Configuration;

public abstract class TransientKeyedConfigRepository<T>(IKeyedConfig configuration) : IKeyedConfigRepository<T>
{
    protected abstract T Make(IKeyedConfig c);

    public T Instance => Make(configuration);
}
