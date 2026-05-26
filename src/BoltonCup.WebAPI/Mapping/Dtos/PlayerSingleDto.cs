namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single player, including game-by-game and tournament stats.</summary>
public record PlayerSingleDto : PlayerDto
{
    /// <summary>Gets or sets the player's formatted height (e.g. "6'1").</summary>
    public string? Height { get; set; }
    /// <summary>Gets or sets the player's weight.</summary>
    public int? Weight { get; set; }
    /// <summary>Gets the player's game-by-game stat history.</summary>
    public List<PlayerGameByGame> GameByGame { get; init; } = [];
    /// <summary>Gets the player's aggregated stats per tournament.</summary>
    public List<PlayerTournamentStats> TournamentStats { get; init; } = [];
}

/// <summary>DTO representing a player's aggregated stats for a single tournament.</summary>
public sealed record PlayerTournamentStats
{
    /// <summary>Gets or sets the number of games played.</summary>
    public required int GamesPlayed { get; set; }
    /// <summary>Gets or sets the number of goals scored.</summary>
    public required int Goals { get; set; }
    /// <summary>Gets or sets the number of assists.</summary>
    public required int Assists { get; set; }
    /// <summary>Gets the total points (goals plus assists).</summary>
    public int Points => Goals + Assists;
    /// <summary>Gets or sets the total penalty minutes.</summary>
    public required int PenaltyMinutes { get; set; }
    /// <summary>Gets or sets the number of wins (goalie stat).</summary>
    public required int Wins { get; set; }
    /// <summary>Gets or sets the number of shutouts (goalie stat).</summary>
    public required int Shutouts { get; set; }
    /// <summary>Gets or sets the goals against average (goalie stat).</summary>
    public required double? GoalsAgainstAverage { get; set; }
    /// <summary>Gets or sets the tournament these stats are from.</summary>
    public required TournamentBriefDto Tournament { get; set; }
    /// <summary>Gets or sets the team the player was on during this tournament.</summary>
    public required TeamBriefDto? Team { get; set; }
}

/// <summary>DTO representing a player's stats for a single game.</summary>
public sealed record PlayerGameByGame
{
    /// <summary>Gets or sets the number of goals scored in the game.</summary>
    public required int Goals  { get; set; }
    /// <summary>Gets or sets the number of assists in the game.</summary>
    public required int Assists  { get; set; }
    /// <summary>Gets the total points (goals plus assists) for the game.</summary>
    public  int Points => Goals + Assists;
    /// <summary>Gets or sets the penalty minutes accumulated in the game.</summary>
    public required int PenaltyMinutes { get; set; }
    /// <summary>Gets or sets whether the player's team won the game.</summary>
    public required bool Win { get; set; }
    /// <summary>Gets or sets the number of shutouts recorded in the game (goalie stat).</summary>
    public required int Shutouts { get; set; }
    /// <summary>Gets or sets the goals allowed in the game (goalie stat).</summary>
    public required int GoalsAgainst { get; set; }
    /// <summary>Gets or sets the tournament this game was part of.</summary>
    public required TournamentBriefDto Tournament { get; set; }
    /// <summary>Gets or sets the game details from the team's perspective.</summary>
    public required GameOfTeamDto Game { get; set; }
    /// <summary>Gets or sets the team the player was on for this game.</summary>
    public required TeamBriefDto? Team { get; set; }
}