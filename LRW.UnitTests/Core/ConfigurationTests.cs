using FakeItEasy;
using FluentValidation;
using LRW.Core.Configuration;

namespace LRW.UnitTests.Core;

public class ConfigurationTests
{
    #region Keys Definitions

    private class KeyWithNoValidation(string name = "TEST", string defaultValue = "TEST") : Key(name, defaultValue, []);

    private class KeyWithRequiredValidation : Key
    {
        public KeyWithRequiredValidation()
            : base("TEST", "")
        {
            RuleFor(x => x.String).NotEmpty();
        }
    }

    private class KeyWithNumberValidation : Key
    {
        public KeyWithNumberValidation()
            : base("TEST", "1")
        {
            RuleFor(x => x.Int).GreaterThan(0).LessThan(100);
        }
    }

    #endregion

    #region Key

    [Theory]
    [InlineData("A")]
    [InlineData("A_B")]
    [InlineData("A_B_C")]
    [InlineData("ABC")]
    public void NewInstanceOf_Key_WhenValidName(string name)
    {
        Assert.Equal(name, new KeyWithNoValidation(name).Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A_")]
    [InlineData("_A")]
    [InlineData("A_1")]
    [InlineData("A_*")]
    [InlineData("A__")]
    [InlineData("A B")]
    [InlineData(" A")]
    public void NewInstanceOf_Key_ThrowArgumentException_WhenInvalidName(string name)
    {
        Assert.Throws<ArgumentException>(() => new KeyWithNoValidation(name));
    }

    #endregion

    #region KeyedConfiguration

    [Fact]
    public void String_KeyedConfiguration_GetDefaultValue_WhenSourceNotFoundKey()
    {
        //Arrange
        var key = new KeyWithNoValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns("");

        var keyedConfiguration = new KeyedConfig(source);

        //Act
        var value = keyedConfiguration[key];

        //Assert
        Assert.Equal(key.DefaultValue, value.String);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void String_KeyedConfiguration_ThrowValidationException_WhenNotMatchRules()
    {
        //Arrange
        var key = new KeyWithRequiredValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns("");

        var keyedConfiguration = new KeyedConfig(source);

        //Assert
        Assert.Throws<ValidationException>(() => keyedConfiguration[key]);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData("True")]
    [InlineData("TrUe")]
    [InlineData("tRuE")]
    public void Bool_KeyedConfiguration_ReturnsTrue_WhenTrueStringCaseInsensitive(string value)
    {
        //Arrange
        var key = new KeyWithNoValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyedConfiguration = new KeyedConfig(source);

        //Act
        var result = keyedConfiguration[key];

        //Assert
        Assert.True(result.Bool);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("")]
    [InlineData("false")]
    [InlineData("p")]
    [InlineData("*")]
    [InlineData("null")]
    [InlineData("true ")]
    [InlineData(" true")]
    public void Bool_KeyedConfiguration_ReturnsFalse_WhenAnyStringOtherThanTrue(string value)
    {
        //Arrange
        var key = new KeyWithNoValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyedConfiguration = new KeyedConfig(source);

        //Act
        var result = keyedConfiguration[key];

        //Assert
        Assert.False(result.Bool);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    public void Int_KeyedConfiguration_ReturnsCorrectNumberConversion(string number)
    {
        //Arrange
        var key = new KeyWithNoValidation(defaultValue: "1");
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(number);

        var keyedConfiguration = new KeyedConfig(source);

        //Act
        var result = keyedConfiguration[key];

        //Assert
        Assert.Equal(1, result.Int);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("One")]
    [InlineData("1.34e28")]
    [InlineData("-26.87")]
    [InlineData("1601.9")]
    [InlineData("2147483648")]
    public void Int_KeyedConfiguration_ThrowValidationException_WhenNotAInteger(string value)
    {
        //Arrange
        var key = new KeyWithNumberValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyedConfiguration = new KeyedConfig(source);

        //Assert
        Assert.Throws<ValidationException>(() => keyedConfiguration[key]);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("0")]
    [InlineData(" 0")]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("2147483647")]
    public void Int_KeyedConfiguration_ThrowValidationException_WhenIntegerNotMatchRule(string value)
    {
        //Arrange
        var key = new KeyWithNumberValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyedConfiguration = new KeyedConfig(source);

        //Assert
        Assert.Throws<ValidationException>(() => keyedConfiguration[key]);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("[\"A\"]")]
    [InlineData("[\"A\", \"B\"]")]
    public void Strings_KeyedConfiguration_ReturnsValidStringArray(string json)
    {
        //Arrange
        var key = new KeyWithNoValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(json);

        var keyedConfiguration = new KeyedConfig(source);

        //Act
        var result = keyedConfiguration[key];

        //Assert
        Assert.NotEmpty(result.Strings);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("")]
    [InlineData("[")]
    [InlineData("string")]
    [InlineData("{}")]
    [InlineData("{\"A\", \"B\"}")]
    [InlineData("{\"A\": [{ \"B\" }]")]
    [InlineData("[{}, {}, {}]")]
    [InlineData("[1, 2, 3, 4, 5]")]
    [InlineData("[\"A\", \"B\", 3]")]
    public void Strings_KeyedConfiguration_ThrowValidationException_WhenInvalidJsonStringArray(string value)
    {
        //Arrange
        var key = new KeyWithNoValidation(defaultValue: "");
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyedConfiguration = new KeyedConfig(source);

        //Act
        var result = keyedConfiguration[key];

        //Assert
        Assert.Throws<ValidationException>(() => result.Strings);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region KeyedConfigRepository

    private class MySingletonKeyedConfig(IKeyedConfig configuration) : SingletonKeyedConfigRepository<string>(configuration)
    {
        protected override string Make(IKeyedConfig c) => c[new KeyWithNoValidation()].String;
    }

    private class MyTransientKeyedConfig(IKeyedConfig configuration) : TransientKeyedConfigRepository<string>(configuration)
    {
        protected override string Make(IKeyedConfig c) => c[new KeyWithNoValidation()].String;
    }

    [Fact]
    public void Instance_SingletonKeyedConfig_MustBeSameForAllInstances()
    {
        //Arrange
        var configuration = A.Fake<IKeyedConfig>();
        var source = new MySingletonKeyedConfig(configuration);

        A.CallTo(() => configuration[A<Key>.Ignored]).ReturnsNextFromSequence(new Value("First"), new Value("Second"));

        //Act
        var result1 = source.Instance;
        var result2 = source.Instance;

        //Assert
        Assert.Equal("First", result1);
        Assert.Equal("First", result2);

        A.CallTo(() => configuration[A<Key>.Ignored]).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Instance_TransientKeyedConfig_MustCreateNewInstance()
    {
        //Arrange
        var configuration = A.Fake<IKeyedConfig>();
        var source = new MyTransientKeyedConfig(configuration);

        A.CallTo(() => configuration[A<Key>.Ignored]).ReturnsNextFromSequence(new Value("First"), new Value("Second"));

        //Act
        var result1 = source.Instance;
        var result2 = source.Instance;

        //Assert
        Assert.Equal("First", result1);
        Assert.Equal("Second", result2);

        A.CallTo(() => configuration[A<Key>.Ignored]).MustHaveHappenedTwiceExactly();
    }

    #endregion
}
