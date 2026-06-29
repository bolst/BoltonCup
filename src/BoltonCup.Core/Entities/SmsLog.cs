namespace BoltonCup.Core;

public class SmsLog : EntityBase
{
    public int Id { get; set; }

    /// <summary>Destination phone number in E.164 format.</summary>
    public required string Recipient { get; set; }

    public required string Body { get; set; }

    /// <summary>True when the provider accepted the message; false when the attempt threw.</summary>
    public bool Succeeded { get; set; }

    public string? Error { get; set; }
}
