namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to create a new custom player ranking for a tournament.</summary>
public record CreateCustomRankingRequest
{
    /// <summary>Gets or sets the ID of the tournament this ranking belongs to.</summary>
    public required int TournamentId { get; set; }

    /// <summary>Gets or sets the display title of the ranking.</summary>
    public required string Title { get; set; }
}