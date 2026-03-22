namespace BoltonCup.WebAPI.Mapping;

public record PlayerSingleDto : PlayerDto
{
    public List<PlayerGameByGame> GameByGame { get; init; } = [];
    public List<PlayerTournamentStats> TournamentStats { get; init; } = [];
}

public sealed record PlayerTournamentStats
{
    public required int GamesPlayed { get; set; }
    public required int Goals { get; set; }
    public required int Assists { get; set; }
    public int Points => Goals + Assists;       
    public required int PenaltyMinutes { get; set; }
    public required int Wins { get; set; }
    public required int Shutouts { get; set; }
    public required double? GoalsAgainstAverage { get; set; }
    public required TournamentBriefDto Tournament { get; set; }
    public required TeamBriefDto? Team { get; set; }
}

public sealed record PlayerGameByGame
{
    public required int Goals  { get; set; }
    public required int Assists  { get; set; }
    public  int Points => Goals + Assists;
    public required int PenaltyMinutes { get; set; }
    public required bool Win { get; set; }
    public required int Shutouts { get; set; }
    public required int GoalsAgainst { get; set; }
    public required TournamentBriefDto Tournament { get; set; }
    public required GameOfTeamDto Game { get; set; }
    public required TeamBriefDto? Team { get; set; }
}