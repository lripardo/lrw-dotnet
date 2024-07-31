namespace LRW.Core.Configuration;

public sealed class DictionaryConfigSource() : Dictionary<string, string>, IConfigSource
{
    public string Get(string key)
    {
        TryGetValue(key, out var value);
        return value ?? "";
    }

    public Task<string> GetAsync(string key) => Task.FromResult(Get(key));
}
