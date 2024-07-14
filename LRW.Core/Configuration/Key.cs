using FluentValidation;
using System.Text.RegularExpressions;

namespace LRW.Core.Configuration;

public abstract partial class Key : AbstractValidator<Value>
{
    [GeneratedRegex("^[A-Z]+(_?[A-Z]+)*$")]
    private static partial Regex NameRegex();

    public string Name { get; }
    public string DefaultValue { get; }
    public string[] Documentation { get; }

    public Key(string name, string defaultValue, string[] documentation)
    {
        if (!NameRegex().IsMatch(name))
        {
            throw new ArgumentException($"Invalid key name {name}");
        }

        Name = name;
        DefaultValue = defaultValue;
        Documentation = documentation;
    }
}
