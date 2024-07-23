namespace LRW.Core.Configuration;

public abstract class SingletonKeyedConfigRepository<T>(IKeyedConfig configuration) : IKeyedConfigRepository<T>
{
    private static T? _instance;

    protected abstract T Make(IKeyedConfig c);

    public T Instance => _instance ??= Make(configuration);
}
