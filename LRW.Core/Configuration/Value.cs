using System.Reflection;
using FluentValidation;
using LRW.Core.Helpers;

namespace LRW.Core.Configuration;

public sealed class Value(string value)
{
    public string String { get; } = value;
    public int Int => ThrowValidationExceptionIfConversionFails(String, Convert.ToInt32);
    public bool Bool => string.Compare(String, "true", StringComparison.OrdinalIgnoreCase) == 0;
    public string[] Strings => ThrowValidationExceptionIfConversionFails(String, (s) => JsonHelper.Deserialize<string[]>(s) ?? []);

    private static T ThrowValidationExceptionIfConversionFails<T>(string value, Func<string, T> converter)
    {
        try
        {
            return converter(value);
        }
        catch (Exception ex)
        {
            var validationException = new ValidationException($"Cannot convert string '{value}' to type {typeof(T).FullName}");

            //TODO: FluentValidation library must implements inner exception constructor to avoid this reflection
            typeof(Exception).GetField("_innerException", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(validationException, ex);

            throw validationException;
        }
    }
}
