namespace BoltonCup.WebAPI.Mapping;

public record SkaterGameLogDto
{
    public required PlayerBriefDto Player { get; init; }
    public required TeamBriefDto Team { get; init; }
    public int OpponentTeamId { get; init; }
    public int GameId { get; init; }
    public int Goals { get; init; }
    public int Assists { get; init; }
    public int Points => Goals + Assists;
    public double PenaltyMinutes { get; init; }
}