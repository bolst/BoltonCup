namespace BoltonCup.Core;

public class EmailLog : EntityBase
{
    public int Id { get; set; }
    public required string Recipient { get; set; }
    public required string Subject { get; set; }
    public required string TemplateName { get; set; }

    /// <summary>True when Resend accepted the message; false when the attempt threw.</summary>
    public bool Succeeded { get; set; }

    public string? Error { get; set; }

    /// <summary>Correlates recipients of a single admin broadcast back to its <see cref="SentEmail"/> row.</summary>
    public Guid? BroadcastId { get; set; }
}
