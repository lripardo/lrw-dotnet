namespace LRW.Core.Configuration;

public class DictionaryConfigSource(Dictionary<string, string> config) : IConfigSource
{
    public string Get(string key)
    {
        return config[key];
    }
}
