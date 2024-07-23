namespace LRW.Core.Configuration;

public interface IKeyedConfig
{
    Value this[Key key] { get; }
}
