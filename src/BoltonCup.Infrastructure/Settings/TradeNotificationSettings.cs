namespace BoltonCup.Infrastructure.Settings;

public sealed class TradeNotificationSettings
{
    /// <summary>When false, trade lifecycle emails (proposed/accepted/declined/cancelled/approved) are not sent. SMS is unaffected. Defaults to true.</summary>
    public bool EmailEnabled { get; set; } = true;
}