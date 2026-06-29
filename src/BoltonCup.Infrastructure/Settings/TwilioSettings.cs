namespace BoltonCup.Infrastructure.Settings;

public sealed class TwilioSettings
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>Twilio sending number in E.164 format (e.g. +15555550123).</summary>
    public string FromPhoneNumber { get; set; } = string.Empty;
}
