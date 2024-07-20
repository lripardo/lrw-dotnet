namespace LRW.Core.Configuration;

public sealed class EnvVarConfigSource : IConfigSource
{
    public string Get(string key)
    {
        return Environment.GetEnvironmentVariable(key) ?? "";
    }

    public Task<string> GetAsync(string key)
    {
        return Task.FromResult(Get(key));
    }
}
