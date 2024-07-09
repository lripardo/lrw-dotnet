namespace LRW.Configuration;

public interface IConfigSource
{
    string Get(string key);
}