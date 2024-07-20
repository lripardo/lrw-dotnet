namespace LRW.Core.Configuration;

public sealed class DictionaryConfigSource(Dictionary<string, string> config) : IConfigSource
{
    public string Get(string key)
    {
        return config[key];
    }

    public Task<string> GetAsync(string key)
    {
        return Task.FromResult(Get(key));
    }
}
