namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a player's position within a custom ranking.</summary>
public sealed record CustomRankingPlayerDto
{
    /// <summary>Gets or sets the player's rank (1-based).</summary>
    public int Rank { get; set; }
    /// <summary>Gets or sets the player being ranked.</summary>
    public required PlayerBriefDto Player { get; set; }
    /// <summary>Gets or sets the number of games played by the player.</summary>
    public int GamesPlayed { get; set; }
    /// <summary>Gets or sets the player's total points.</summary>
    public int TotalPoints { get; set; }
    /// <summary>Gets or sets the player's points per game average.</summary>
    public double PointsPerGame { get; set; }
}
