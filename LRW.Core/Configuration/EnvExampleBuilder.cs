using System.Text;

namespace LRW.Core.Configuration;

public static class EnvExampleBuilder
{
    private static readonly Type Key = typeof(Key);

    public static string Build(IEnumerable<Type> implementations, char newLine)
    {
        var builder = new StringBuilder($"# Automatically generated file, do not edit.");

        builder.Append(newLine);
        builder.Append(newLine);

        foreach (var implementation in implementations)
        {
            if (implementation.IsSubclassOf(Key))
            {
                var key =
                    (Key?)Activator.CreateInstance(implementation)
                    ?? throw new Exception($"Cannot create Key instance from type {implementation.FullName} because Activator.CreateInstance() return null");

                foreach (var documentation in key.Documentation)
                {
                    builder.Append($"# {documentation}");
                    builder.Append(newLine);
                }

                builder.Append($"{key.Name}={key.DefaultValue}");

                builder.Append(newLine);
                builder.Append(newLine);
            }
        }

        return builder.ToString();
    }
}
