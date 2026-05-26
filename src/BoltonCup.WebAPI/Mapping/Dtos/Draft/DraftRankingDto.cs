namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a player's ranking in a draft pool.</summary>
public sealed record DraftRankingDto
{
    /// <summary>Gets or sets the draft ranking record ID.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the ID of the draft this ranking belongs to.</summary>
    public int DraftId { get; set; }
    /// <summary>Gets or sets the ID of the tournament this ranking belongs to.</summary>
    public required int TournamentId { get; set; }
    /// <summary>Gets or sets the player's phone number.</summary>
    public string? PlayerPhone { get; set; }
    /// <summary>Gets or sets the player being ranked.</summary>
    public required PlayerBriefDto Player { get; set; }
    /// <summary>Gets or sets the draft pick used to select this player, if drafted.</summary>
    public DraftPickBriefDto? DraftPick { get; set; }
    /// <summary>Gets or sets the number of games played by the player.</summary>
    public int GamesPlayed { get; set; }
    /// <summary>Gets or sets the player's total points.</summary>
    public int TotalPoints { get; set; }
    /// <summary>Gets or sets the player's calculated draft ranking score.</summary>
    public double DraftRanking { get; set; }
    /// <summary>Gets or sets whether the ranking has been manually overridden.</summary>
    public bool OverrideRanking { get; set; }
    /// <summary>Gets or sets whether the player has already been drafted.</summary>
    public bool IsDrafted { get; set; }
    /// <summary>Gets or sets the player's points per game average.</summary>
    public double PointsPerGame { get; set; }
}