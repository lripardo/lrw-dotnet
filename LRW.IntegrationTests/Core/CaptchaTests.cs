using FluentValidation;
using LRW.Core.Captcha;
using LRW.Core.Communication;
using LRW.Core.Configuration;
using LRW.Core.Date;
using Xunit.Abstractions;

namespace LRW.IntegrationTests.Core;

public class CaptchaTests(ITestOutputHelper output)
{
    private const string ValidRemoteIp = "127.0.0.1";
    private const string ValidResponse = "10000000-aaaa-bbbb-cccc-000000000001";
    private const string ValidSiteKey = "10000000-ffff-ffff-ffff-000000000001";
    private const string ValidSecret = "0x0000000000000000000000000000000000000000";

    [Fact]
    public async Task HCaptcha_Validate_MustNotThrowExceptionWhenTokenIsValid()
    {
        //Arrange
        var src = new DictionaryConfigSource() { { "HCAPTCHA_SECRET", ValidSecret } };
        var hcaptcha = new HCaptchaValidator(new KeyedConfig(src), new XUnitLogger(output), new SystemDateTimeResolver());

        //Act and Assert
        await hcaptcha.Validate(new HCaptchaInput() { RemoteIp = ValidRemoteIp, Response = ValidResponse, SiteKey = ValidSiteKey });
    }

    [Theory]
    [InlineData("")]
    [InlineData("any invalid key")]
    public async Task HCaptcha_Validate_MustThrowValidationExceptionOnInvalidResponses(string response)
    {
        //Arrange
        var src = new DictionaryConfigSource() { { "HCAPTCHA_SECRET", ValidSecret } };
        var hcaptcha = new HCaptchaValidator(new KeyedConfig(src), new XUnitLogger(output), new SystemDateTimeResolver());

        //Act and Assert
        await Assert.ThrowsAsync<ValidationException>(() => hcaptcha.Validate(new HCaptchaInput() { Response = response, SiteKey = ValidSiteKey, RemoteIp = ValidRemoteIp }));
    }

    [Fact]
    public async Task HCaptcha_Validate_MustThrowValidationExceptionOnInvalidSiteKey()
    {
        //Arrange
        var src = new DictionaryConfigSource() { { "HCAPTCHA_SECRET", ValidSecret }, { "HCAPTCHA_VERIFY_SITE_KEY", "ANY DIFFERENT KEY"} };
        var hcaptcha = new HCaptchaValidator(new KeyedConfig(src), new XUnitLogger(output), new SystemDateTimeResolver());

        //Act and Assert
        await Assert.ThrowsAsync<ValidationException>(() => hcaptcha.Validate(new HCaptchaInput() { Response = ValidResponse, SiteKey = ValidSiteKey, RemoteIp = ValidRemoteIp }));
    }
}
