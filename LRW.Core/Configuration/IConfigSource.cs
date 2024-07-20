namespace LRW.Core.Configuration;

public interface IConfigSource
{
    string Get(string key);
    Task<string> GetAsync(string key);
}
