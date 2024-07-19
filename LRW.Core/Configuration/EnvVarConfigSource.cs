namespace LRW.Core.Configuration;

public sealed class EnvVarConfigSource : IConfigSource
{
    public string Get(string key)
    {
        return Environment.GetEnvironmentVariable(key) ?? "";
    }
}
