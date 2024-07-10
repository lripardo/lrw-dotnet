using System.Reflection;
using System.Text;

namespace LRW.Configuration;

public static class EnvExampleBuilder
{
    private static string Build(Assembly assembly, char newLine)
    {
        var implementations = assembly.DefinedTypes.Where(t => t.IsSubclassOf(typeof(Key)));
        var builder = new StringBuilder($"# Automatically generated file, do not edit.");
        
        builder.Append(newLine);
        builder.Append(newLine);

        foreach (var implementation in implementations)
        {
            var key = (Key?)Activator.CreateInstance(implementation) ?? throw new Exception($"Cannot create Key instance from type {implementation.FullName} because Activator.CreateInstance() return null");

            foreach (var documentation in key.Documentation)
            {
                builder.Append($"# {documentation}");
                builder.Append(newLine);
            }

            builder.Append($"{key.Name}={key.DefaultValue}");
            
        builder.Append(newLine);
            builder.Append(newLine);
        }

        return builder.ToString();
    }

    public static string BuildUnix(Assembly assembly)
    {
        return Build(assembly, '\n');
    }
}
