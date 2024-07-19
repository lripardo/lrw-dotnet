using FluentValidation;

namespace LRW.Core.Configuration;

public sealed class KeyConfiguration(IConfigSource source) : IKeyConfiguration
{
    public Value this[Key key]
    {
        get
        {
            var valueString = source.Get(key.Name);

            if (string.IsNullOrEmpty(valueString))
            {
                valueString = key.DefaultValue;
            }

            var value = new Value(valueString);

            key.ValidateAndThrow(value);

            return value;
        }
    }
}
