namespace BoltonCup.WebAPI.Mapping.Core;

public record GoalieGameLogDto
{
    public required PlayerBriefDto Player { get; init; }
    public required TeamBriefDto Team { get; init; }
    public int OpponentTeamId { get; init; }
    public int GameId { get; init; }
    public int Goals { get; init; }
    public int Assists { get; init; }
    public int Points => Goals + Assists;
    public double PenaltyMinutes { get; init; }
    public required int GoalsAgainst { get; init; }
    public required double GoalsAgainstAverage { get; init; }
    public required int ShotsAgainst { get; init; }
    public required int Saves { get; init; }
    public required double SavePercentage { get; init; }
    public required bool Shutout { get; init; }
    public required bool Win { get; init; }
}