using FluentValidation;
using LRW.Core.Configuration;
using LRW.Core.Date;
using LRW.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace LRW.Core.Captcha;

public sealed class HCaptchaValidator(IKeyedConfig config, ILogger logger, IDateTimeResolver date) : ICaptchaValidator<HCaptchaInput>
{
    public class HCaptchaResult
    {
        public bool Success { get; init; }
        public DateTime ChallengeTs { get; init; }
        public string Hostname { get; init; } = string.Empty;
    }

    public class Secret : Key
    {
        public Secret() : base("HCAPTCHA_SECRET", "")
        {
            RuleFor(x => x.String).NotEmpty();
        }
    }

    public class VerifyUrl : Key
    {
        public VerifyUrl() : base("HCAPTCHA_VERIFY_URL", "https://api.hcaptcha.com/siteverify")
        {
            RuleFor(x => x.String).NotEmpty();
        }
    }

    public class ClientHostname() : Key("HCAPTCHA_CLIENT_HOSTNAME", "");

    public class ExpirationChallenge : Key
    {
        public ExpirationChallenge() : base("HCAPTCHA_EXPIRATION_CHALLENGE", "0", ["Time in seconds", "Value 0 means no expiration check"])
        {
            RuleFor(x => x.Int).GreaterThanOrEqualTo(0).LessThanOrEqualTo(86400);
        }
    }

    public class VerifySiteKey() : Key("HCAPTCHA_VERIFY_SITE_KEY", "");

    private readonly string _secret = config[new Secret()].String;
    private readonly string _verifyUrl = config[new VerifyUrl()].String;
    private readonly string _clientHostname = config[new ClientHostname()].String;
    private readonly int _expirationChallengeInSeconds = config[new ExpirationChallenge()].Int;
    private readonly string _verifySiteKey = config[new VerifySiteKey()].String;

    public async Task Validate(HCaptchaInput input)
    {
        if (string.IsNullOrEmpty(input.Response))
        {
            throw new ValidationException("Empty input response (HCaptcha challenge token)");
        }

        if (!string.IsNullOrEmpty(_verifySiteKey) && input.SiteKey != _verifySiteKey)
        {
            throw new ValidationException($"The site key {input.SiteKey} is different from configured site key {_verifySiteKey}");
        }

        var payload = new Dictionary<string, string> { ["secret"] = _secret, ["response"] = input.Response };

        if (!string.IsNullOrEmpty(input.RemoteIp))
        {
            payload.Add("remoteip", input.RemoteIp);
        }

        if (!string.IsNullOrEmpty(input.SiteKey))
        {
            payload.Add("sitekey", input.SiteKey);
        }

        var response = await new HttpClient().PostAsync(_verifyUrl, new FormUrlEncodedContent(payload));

        var body = await response.Content.ReadAsStringAsync();

        logger.LogDebug("Raw JSON from HCaptcha server: {body}", body);

        var hCaptchaResult = JsonHelper.Deserialize<HCaptchaResult>(body);

        if (hCaptchaResult == null) throw new NullReferenceException();

        if (!hCaptchaResult.Success)
        {
            throw new ValidationException("The HCaptcha service do not recognize this challenge");
        }

        if (!string.IsNullOrEmpty(_clientHostname) && hCaptchaResult.Hostname != _clientHostname)
        {
            throw new ValidationException($"The HCaptcha hostname {hCaptchaResult.Hostname} is different from configured hostname {_clientHostname}");
        }

        if (_expirationChallengeInSeconds != 0)
        {
            var expiresAt = hCaptchaResult.ChallengeTs.AddSeconds(_expirationChallengeInSeconds);

            if (date.Now > expiresAt)
            {
                throw new ValidationException($"The challenge has expired at {expiresAt}");
            }
        }
    }
}
