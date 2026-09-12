namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single custom ranking, including its ordered players.</summary>
public sealed record CustomRankingSingleDto
{
    /// <summary>Gets or sets the custom ranking ID.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the ranking title.</summary>
    public required string Title { get; set; }
    /// <summary>Gets or sets the tournament this ranking belongs to.</summary>
    public required TournamentBriefDto Tournament { get; set; }
    /// <summary>Gets or sets the display name of the account that created the ranking.</summary>
    public required string CreatedByName { get; set; }
    /// <summary>Gets or sets the ranked players ordered by rank.</summary>
    public required List<CustomRankingPlayerDto> Players { get; set; }
    /// <summary>Gets or sets whether the current caller can edit the ranking (owner or admin). Shared viewers see false.</summary>
    public bool CanEdit { get; set; }
}