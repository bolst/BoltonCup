namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to update an existing custom player ranking.</summary>
public record UpdateCustomRankingRequest
{
    /// <summary>Gets or sets the updated display title for the ranking.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the players in their new ranked order (must match the existing player set).</summary>
    public List<int>? OrderedPlayerIds { get; set; }
}
