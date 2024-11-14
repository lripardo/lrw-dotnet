using FluentValidation;

namespace LRW.Core.Configuration;

public abstract class Key : AbstractValidator<Value>
{
    public string Name { get; }
    public string DefaultValue { get; }
    public string[] Documentation { get; }
    public string Version { get; }
    public string Type { get; private set; } = nameof(String);

    private bool _typeAlreadyModified;

    private sealed class KeyValidator : AbstractValidator<Key>
    {
        public KeyValidator()
        {
            RuleFor(x => x.Name).NotEmpty().Matches("^[A-Z]+(_?[A-Z]+)*$");
            RuleFor(x => x.Version).NotEmpty();
        }
    }

    protected Key(string name, string defaultValue, string[]? documentation = null, string version = "1.0.0")
    {
        Name = name;
        DefaultValue = defaultValue;
        Documentation = documentation ?? [];
        Version = version;

        new KeyValidator().ValidateAndThrow(this);
    }

    protected override void OnRuleAdded(IValidationRule<Value> rule)
    {
        rule.PropertyName = "KeyValidationError";

        //TODO: Change this when update FluentValidation to 12.0.0
        //https://github.com/FluentValidation/FluentValidation/issues/2179
        rule.GetType().GetMethod("SetDisplayName", [typeof(string)])?.Invoke(rule, [Name]);

        if (rule.TypeToValidate.Name != Type)
        {
            if (_typeAlreadyModified)
            {
                throw new ValidationException("The Type property can modified only once");
            }

            Type = rule.TypeToValidate.Name;
        }

        _typeAlreadyModified = true;
    }
}
