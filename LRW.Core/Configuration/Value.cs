using System.Reflection;
using FluentValidation;
using LRW.Core.Helpers;

namespace LRW.Core.Configuration;

public class Value(string value)
{
    public string String { get; } = value;
    public int Int => ThrowValidationExceptionIfCathAnotherException(String, Convert.ToInt32);
    public bool Bool => ThrowValidationExceptionIfCathAnotherException(String, (v) => string.Compare(v, "true", StringComparison.OrdinalIgnoreCase) == 0);
    public string[] Strings => ThrowValidationExceptionIfCathAnotherException(String, (v) => JsonHelper.Deserialize<string[]>(v) ?? []);

    private static T ThrowValidationExceptionIfCathAnotherException<T>(string input, Func<string, T> action)
    {
        try
        {
            return action(input);
        }
        catch (Exception ex)
        {
            var validationException = new ValidationException($"Cannot convert value {input} to {typeof(T).Name}");

            //TODO: FluentValidation must implements inner exception constructor to avoid this reflection
            typeof(Exception).GetField("_innerException", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(validationException, ex);

            throw validationException;
        }
    }
}
