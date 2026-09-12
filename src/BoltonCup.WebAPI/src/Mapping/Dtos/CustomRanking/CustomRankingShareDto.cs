namespace BoltonCup.WebAPI.Mapping;

/// <summary>An account a custom ranking is shared with (view-only).</summary>
public sealed record CustomRankingShareDto
{
    /// <summary>Gets or sets the shared-with account ID.</summary>
    public int AccountId { get; set; }
    /// <summary>Gets or sets the account's display name.</summary>
    public required string Name { get; set; }
    /// <summary>Gets or sets the account's email.</summary>
    public required string Email { get; set; }
    /// <summary>Gets or sets the account's avatar URL.</summary>
    public string? Avatar { get; set; }
}