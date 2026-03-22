namespace BoltonCup.WebAPI.Mapping;

public sealed record AccountTournamentDto
{
    public required TournamentBriefDto Tournament { get; init; }
    public TeamBriefDto? Team { get; init; }
}