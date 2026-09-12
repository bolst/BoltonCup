namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of teams with optional filters.</summary>
public record GetTeamsRequest : RequestBase
{
    /// <summary>Gets or sets an optional tournament ID to filter teams by.</summary>
    public int? TournamentId { get; set; }
}