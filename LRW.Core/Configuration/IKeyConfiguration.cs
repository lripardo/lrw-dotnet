namespace LRW.Configuration;

public interface IKeyConfiguration
{
    Value this[Key key] { get; }
}