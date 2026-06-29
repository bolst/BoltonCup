namespace BoltonCup.Core;

public class EmailDraft : EntityBase
{
    public int Id { get; set; }
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public bool UseLayout { get; set; } = true;

    /// <summary>Enum name of the composer's audience selection (e.g. "PlayerPool").</summary>
    public required string AudienceKind { get; set; }

    /// <summary>Tournament context for PlayerPool/Unpaid audiences; null for AllAccounts.</summary>
    public int? TournamentId { get; set; }
}
