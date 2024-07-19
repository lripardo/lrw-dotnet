using FakeItEasy;
using FluentValidation;
using LRW.Core.Configuration;

namespace LRW.Tests.Core;

public class ConfigurationUnitTests
{
    #region Keys Definitions

    private class KeyWithNoValidation(string name = "TEST", string defaultValue = "TEST") : Key(name, defaultValue, []);

    private class KeyWithRequiredValidation : Key
    {
        public KeyWithRequiredValidation()
            : base("TEST", "", [])
        {
            RuleFor(x => x.String).NotEmpty();
        }
    }

    private class KeyWithNumberValidation : Key
    {
        public KeyWithNumberValidation()
            : base("TEST", "1", [])
        {
            RuleFor(x => x.Int).GreaterThan(0).LessThan(100);
        }
    }

    private class FakeKey1() : Key("FAKE_KEY_ONE", "1", ["This is a fake key number 1"]);

    private class FakeKey2() : Key("FAKE_KEY_TWO", "2", ["This is a fake key number 2", "And key number 2 has another documentation"]);

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

    #region KeyConfiguration

    [Fact]
    public void String_KeyConfiguration_GetDefaultValue_WhenSourceNotFoundKey()
    {
        //Arrange
        var key = new KeyWithNoValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns("");

        var keyConfiguration = new KeyConfiguration(source);

        //Act
        var value = keyConfiguration[key];

        //Assert
        Assert.Equal(key.DefaultValue, value.String);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void String_KeyConfiguration_ThrowValidationException_WhenNotMatchRules()
    {
        //Arrange
        var key = new KeyWithRequiredValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns("");

        var keyConfiguration = new KeyConfiguration(source);

        //Assert
        Assert.Throws<ValidationException>(() => keyConfiguration[key]);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData("True")]
    [InlineData("TrUe")]
    [InlineData("tRuE")]
    public void Bool_KeyConfiguration_ReturnsTrue_WhenTrueStringCaseInsensitive(string value)
    {
        //Arrange
        var key = new KeyWithNoValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyConfiguration = new KeyConfiguration(source);

        //Act
        var result = keyConfiguration[key];

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
    public void Bool_KeyConfiguration_ReturnsFalse_WhenAnyStringOtherThanTrue(string value)
    {
        //Arrange
        var key = new KeyWithNoValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyConfiguration = new KeyConfiguration(source);

        //Act
        var result = keyConfiguration[key];

        //Assert
        Assert.False(result.Bool);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    public void Int_KeyConfiguration_ReturnsCorrectNumberConversion(string number)
    {
        //Arrange
        var key = new KeyWithNoValidation(defaultValue: "1");
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(number);

        var keyConfiguration = new KeyConfiguration(source);

        //Act
        var result = keyConfiguration[key];

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
    public void Int_KeyConfiguration_ThrowValidationException_WhenNotAInteger(string value)
    {
        //Arrange
        var key = new KeyWithNumberValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyConfiguration = new KeyConfiguration(source);

        //Assert
        Assert.Throws<ValidationException>(() => keyConfiguration[key]);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("0")]
    [InlineData(" 0")]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("2147483647")]
    public void Int_KeyConfiguration_ThrowValidationException_WhenIntegerNotMatchRule(string value)
    {
        //Arrange
        var key = new KeyWithNumberValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyConfiguration = new KeyConfiguration(source);

        //Assert
        Assert.Throws<ValidationException>(() => keyConfiguration[key]);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("[\"A\"]")]
    [InlineData("[\"A\", \"B\"]")]
    public void Strings_KeyConfiguration_ReturnsValidStringArray(string json)
    {
        //Arrange
        var key = new KeyWithNoValidation();
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(json);

        var keyConfiguration = new KeyConfiguration(source);

        //Act
        var result = keyConfiguration[key];

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
    public void Strings_KeyConfiguration_ThrowValidationException_WhenInvalidJsonStringArray(string value)
    {
        //Arrange
        var key = new KeyWithNoValidation(defaultValue: "");
        var source = A.Fake<IConfigSource>();

        A.CallTo(() => source.Get(key.Name)).Returns(value);

        var keyConfiguration = new KeyConfiguration(source);

        //Act
        var result = keyConfiguration[key];

        //Assert
        Assert.Throws<ValidationException>(() => result.Strings);
        A.CallTo(() => source.Get(key.Name)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region EnvExampleBuilder

    [Fact]
    public void BuildUnix_EnvExampleBuilder_ReturnsCorrectStringForEnvExampleFileContent()
    {
        //Arrange
        const string expectedText = ""
            + "# Automatically generated file, do not edit.\n\n"
            + "# This is a fake key number 1\n"
            + "FAKE_KEY_ONE=1\n\n"
            + "# This is a fake key number 2\n"
            + "# And key number 2 has another documentation\n"
            + "FAKE_KEY_TWO=2\n\n";

        //Act
        var envFileContent = EnvExampleBuilder.Build([typeof(FakeKey1), typeof(FakeKey2)], "\n");

        //Assert
        Assert.Equal(expectedText, envFileContent);
    }

    #endregion
}
