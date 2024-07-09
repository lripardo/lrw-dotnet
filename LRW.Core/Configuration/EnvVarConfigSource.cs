namespace LRW.Configuration;

public class EnvVarConfigSource : IConfigSource
{
    public string Get(string key)
    {
        return Environment.GetEnvironmentVariable(key) ?? "";
    }
}