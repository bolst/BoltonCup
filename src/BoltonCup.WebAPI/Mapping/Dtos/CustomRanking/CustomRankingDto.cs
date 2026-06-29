namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a user's custom player ranking.</summary>
public sealed record CustomRankingDto
{
    /// <summary>Gets or sets the custom ranking ID.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the ranking title.</summary>
    public required string Title { get; set; }
    /// <summary>Gets or sets the tournament this ranking belongs to.</summary>
    public required TournamentBriefDto Tournament { get; set; }
    /// <summary>Gets or sets the number of players in the ranking.</summary>
    public int PlayerCount { get; set; }
    /// <summary>Gets or sets the display name of the account that created the ranking.</summary>
    public required string CreatedByName { get; set; }
    /// <summary>Gets or sets when the ranking was created.</summary>
    public DateTime CreatedAt { get; set; }
}