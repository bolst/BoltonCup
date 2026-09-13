namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single team, including its roster.</summary>
public sealed record TeamSingleDto : TeamDto
{
    /// <summary>Gets the list of players on the team's roster.</summary>
    public required List<PlayerBriefDto> Players { get; init; } = [];
}