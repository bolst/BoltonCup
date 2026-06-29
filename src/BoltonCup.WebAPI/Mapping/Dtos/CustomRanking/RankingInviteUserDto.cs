namespace BoltonCup.WebAPI.Mapping;

/// <summary>An account that can be invited to view a ranking (a GM of the tournament).</summary>
public sealed record RankingInviteUserDto
{
    /// <summary>Gets or sets the account ID.</summary>
    public int AccountId { get; set; }
    /// <summary>Gets or sets the account's display name.</summary>
    public required string Name { get; set; }
    /// <summary>Gets or sets the account's email.</summary>
    public required string Email { get; set; }
}
