namespace BoltonCup.Core;

public interface IStandingsService
{
    Task<TournamentStandings> GetStandingsAsync(int tournamentId, CancellationToken cancellationToken = default);
}

/// <summary>Computed standings for a tournament, split by stage.</summary>
public sealed record TournamentStandings
{
    public required IReadOnlyList<StandingRow> RoundRobin { get; init; }
    public required IReadOnlyList<StandingRow> Playoffs { get; init; }
}
