using FluentValidation;

namespace LRW.Core.Configuration;

public sealed class FluentKeyConfiguration(IConfigSource source) : IKeyConfiguration
{
    private readonly IConfigSource _source = source;

    public Value this[Key key]
    {
        get
        {
            var src = _source.Get(key.Name);

            if (string.IsNullOrEmpty(src))
            {
                src = key.DefaultValue;
            }

            var value = new Value(src);

            key.ValidateAndThrow(value);

            return value;
        }
    }
}