namespace BoltonCup.WebAPI.Mapping;

/// <summary>Standings for a tournament, split into round robin and playoff stages.</summary>
public record TournamentStandingsDto
{
    /// <summary>Gets the round robin standings, ranked best to worst.</summary>
    public required IReadOnlyList<StandingRowDto> RoundRobin { get; init; }
    /// <summary>Gets the playoff standings, ranked best to worst.</summary>
    public required IReadOnlyList<StandingRowDto> Playoffs { get; init; }
}