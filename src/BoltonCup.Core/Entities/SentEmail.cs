namespace BoltonCup.Core;

public class SentEmail : EntityBase
{
    public int Id { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public required string Audience { get; set; }
    public int RecipientCount { get; set; }
    public bool UseLayout { get; set; }

    /// <summary>Correlates this broadcast to its per-recipient <see cref="EmailLog"/> rows.</summary>
    public Guid BroadcastId { get; set; }
}
