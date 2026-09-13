namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of games with optional filters.</summary>
public record GetGamesRequest : RequestBase
{
    /// <summary>Gets or sets an optional tournament ID to filter games by.</summary>
    public int? TournamentId { get; set; }

    /// <summary>Gets or sets an optional team ID to filter games by.</summary>
    public int? TeamId { get; set; }
}