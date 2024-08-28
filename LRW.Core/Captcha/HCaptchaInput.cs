namespace LRW.Core.Captcha;

public class HCaptchaInput
{
    public string Response { get; init; } = string.Empty;
    public string? RemoteIp { get; init; }
    public string? SiteKey { get; init; }
}
