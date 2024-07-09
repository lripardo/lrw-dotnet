using FakeItEasy;
using FluentValidation;
using LRW.Configuration;

namespace LRW.Tests;

public class ConfigurationUnitTests
{
    public class KeyWithNoValidation(string name = "TEST", string defaultValue = "TEST") : Key(name, defaultValue)
    {
        public override string[] Documentation => [];
    }

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

    public class KeyWithRequiredValidation : Key
    {
        public KeyWithRequiredValidation() : base("TEST", "")
        {
            RuleFor(x => x.String).NotEmpty();
        }

        public override string[] Documentation => [];
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
    public void Bool_KeyConfiguration_ReturnsFasle_WhenAnyStringOtherThanTrue(string value)
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
}