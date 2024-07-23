using System.Text.Json;
using LRW.Core.Configuration;
using LRW.Core.Helpers;

namespace LRW.UnitTests.Core;

public class HelpersTests
{
    #region JsonHelper

    [Fact]
    public void Serialize_Helpers_ReturnSnakeCaseJsonString()
    {
        //Arrange
        var obj = new
        {
            TestA = "Test",
            TestB = 2,
            TestC = 0.8,
            TestD = true,
            TestE = new { TestA = "Test" },
            TestF = new[] { "A", "B" }
        };

        const string expected =
            "{\"test_a\":\"Test\",\"test_b\":2,\"test_c\":0.8,\"test_d\":true,\"test_e\":{\"test_a\":\"Test\"},\"test_f\":[\"A\",\"B\"]}";

        //Act
        var result = JsonHelper.Serialize(obj);

        //Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Deserialize_Helpers_ReturnObject()
    {
        //Arrange
        const string obj =
            "{\"test_a\":\"Test\",\"test_b\":2,\"test_c\":0.8,\"test_d\":true,\"test_e\":{\"test_a\":\"Test\"},\"test_f\":[\"A\",\"B\"]}";

        //Act
        var result = JsonHelper.Deserialize<JsonDocument>(obj);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Object, result.RootElement.ValueKind);
        Assert.Equal("Test", result.RootElement.GetProperty("test_a").GetString());
        Assert.Equal(2, result.RootElement.GetProperty("test_b").GetInt32());
        Assert.Equal(0.8, result.RootElement.GetProperty("test_c").GetDouble());
        Assert.True(result.RootElement.GetProperty("test_d").GetBoolean());
        Assert.Equal(JsonValueKind.Object, result.RootElement.GetProperty("test_e").ValueKind);
        Assert.Equal("Test", result.RootElement.GetProperty("test_e").GetProperty("test_a").GetString());
        Assert.Equal(JsonValueKind.Array, result.RootElement.GetProperty("test_f").ValueKind);
        Assert.Equal("A", result.RootElement.GetProperty("test_f")[0].GetString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("[")]
    [InlineData(" ")]
    public void Deserialize_Helpers_ThrowExceptionWhenInvalidString(string value)
    {
        Assert.Throws<JsonException>(() => JsonHelper.Deserialize<object?>(value));
    }

    #endregion

    #region EnvExampleBuilder

    private class FakeKey1() : Key("FAKE_KEY_ONE", "1", ["This is a fake key number 1"]);

    private class FakeKey2() : Key("FAKE_KEY_TWO", "2",
        ["This is a fake key number 2", "And key number 2 has another documentation"]);

    [Fact]
    public void GetFileContent_EnvExampleBuilder_ReturnsCorrectContentForUnixEndOfLine()
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
        var envFileContent = EnvExampleBuilder.GetFileContent([typeof(FakeKey1), typeof(FakeKey2)], "\n");

        //Assert
        Assert.Equal(expectedText, envFileContent);
    }

    #endregion
}
