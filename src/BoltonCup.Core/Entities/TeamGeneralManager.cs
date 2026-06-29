namespace BoltonCup.Core;

public class TeamGeneralManager
{
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    /// <summary>Denormalized from the team so a unique (tournament, account) index can enforce one team per GM per tournament.</summary>
    public int? TournamentId { get; set; }
}
