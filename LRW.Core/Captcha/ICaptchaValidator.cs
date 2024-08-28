namespace LRW.Core.Captcha;

public interface ICaptchaValidator<in T>
{
    Task Validate(T input);
}
