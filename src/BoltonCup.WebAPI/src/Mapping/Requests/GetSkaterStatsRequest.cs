namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of skater stats with optional filters.</summary>
public record GetSkaterStatsRequest : RequestBase
{
    /// <summary>Gets or sets an optional tournament ID to filter stats by.</summary>
    public int? TournamentId { get; set; }

    /// <summary>Gets or sets an optional position to filter stats by.</summary>
    public string? Position { get; set; }

    /// <summary>Gets or sets an optional list of team IDs to filter stats by.</summary>
    public List<int>? TeamIds { get; set; }

    /// <summary>Gets or sets an optional game ID to filter stats by.</summary>
    public int? GameId { get; set; }
}