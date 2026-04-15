namespace BoltonCup.Infrastructure.Settings;

public sealed class StripeSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}