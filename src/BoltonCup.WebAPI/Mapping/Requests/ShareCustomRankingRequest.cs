namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to share a custom ranking (view-only) with another account.</summary>
public record ShareCustomRankingRequest
{
    /// <summary>Gets or sets the account being granted view access.</summary>
    public required int AccountId { get; set; }
}
