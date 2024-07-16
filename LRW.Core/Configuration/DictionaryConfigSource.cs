namespace LRW.Core.Configuration;

public class DictionaryConfigSource(Dictionary<string, string> config) : IConfigSource
{
    public Dictionary<string, string> Config { get; set; } = config;

    public string Get(string key)
    {
        return Config[key];
    }
}
