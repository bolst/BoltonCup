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
    /// <summary>Gets or sets the ranked players ordered by rank.</summary>
    public required List<CustomRankingPlayerDto> Players { get; set; }
}
