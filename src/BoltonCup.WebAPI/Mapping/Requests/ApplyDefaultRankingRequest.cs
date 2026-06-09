namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to apply (or clear) a custom ranking as a draft's default player ordering.</summary>
public record ApplyDefaultRankingRequest
{
    /// <summary>Gets or sets the custom ranking ID to apply, or null to reset to the points-per-game default.</summary>
    public int? RankingId { get; set; }
}
