namespace LRW.Core.Configuration;

public interface IKeyConfiguration
{
    Value this[Key key] { get; }
}