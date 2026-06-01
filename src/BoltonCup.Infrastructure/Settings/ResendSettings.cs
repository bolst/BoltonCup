namespace BoltonCup.Infrastructure.Settings;

public sealed class ResendSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
}
