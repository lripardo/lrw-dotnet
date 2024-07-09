namespace LRW.Configuration;

public class DictionaryConfigSource : IConfigSource
{
    public Dictionary<string, string> Config { get; set; } = [];

    public string Get(string key)
    {
        return Config[key];
    }
}