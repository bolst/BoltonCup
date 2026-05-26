namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of players with optional filters.</summary>
public record GetPlayersRequest : RequestBase
{
    /// <summary>Gets or sets an optional tournament ID to filter players by.</summary>
    public int? TournamentId { get; set; }

    /// <summary>Gets or sets an optional team ID to filter players by.</summary>
    public int? TeamId { get; set; }
}