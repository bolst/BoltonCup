namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing one season of a franchise, with the team's as-played brand.</summary>
public record FranchiseSeasonDto
{
    /// <summary>Gets the ID of the team that played this season.</summary>
    public required int TeamId { get; init; }
    /// <summary>Gets the team name as played that season.</summary>
    public required string Name { get; init; }
    /// <summary>Gets the URL of the team logo as played that season.</summary>
    public string? LogoUrl { get; init; }
    /// <summary>Gets the tournament of this season; null when the team has no tournament.</summary>
    public TournamentBriefDto? Tournament { get; init; }
    /// <summary>Gets the team's record for this season.</summary>
    public required FranchiseRecordDto Record { get; init; }
    /// <summary>Gets the team's general managers that season.</summary>
    public required List<TeamGmDto> GeneralManagers { get; init; } = [];
    /// <summary>Gets whether this team won its tournament.</summary>
    public required bool IsChampion { get; init; }
}