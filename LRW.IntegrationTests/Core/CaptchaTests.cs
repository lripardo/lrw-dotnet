using LRW.Core.Captcha;
using LRW.Core.Configuration;
using LRW.Core.Date;
using Xunit.Abstractions;

namespace LRW.IntegrationTests.Core;

public class CaptchaTests(ITestOutputHelper output)
{
    [Fact]
    public async Task HCaptcha_Validate_MustNotThrowExceptionWhenTokenIsValid()
    {
        //Arrange
        var src = new DictionaryConfigSource() { { "HCAPTCHA_SECRET", "0x0000000000000000000000000000000000000000" } };
        var hcaptcha = new HCaptchaValidator(new KeyedConfig(src), new XUnitLogger(output), new SystemDateTimeResolver());

        //Act
        await hcaptcha.Validate(new HCaptchaInput()
        {
            RemoteIp = "127.0.0.1",
            Response = "10000000-aaaa-bbbb-cccc-000000000001",
            SiteKey = "10000000-ffff-ffff-ffff-000000000001",
        });
    }
}
