using System.Text;

namespace LRW.Core.Configuration;

public static class EnvExampleBuilder
{
    public static string Build(IEnumerable<Type> implementations, string newLine)
    {
        var builder = new StringBuilder($"# Automatically generated file, do not edit.");
        builder.Append($"{newLine}{newLine}");

        foreach (var implementation in implementations)
        {
            if (!implementation.IsSubclassOf(typeof(Key))) continue;

            var key = (Key?)Activator.CreateInstance(implementation)!;

            foreach (var documentation in key.Documentation)
            {
                builder.Append($"# {documentation}{newLine}");
            }

            builder.Append($"{key.Name}={key.DefaultValue}{newLine}{newLine}");
        }

        return builder.ToString();
    }
}
