namespace BoltonCup.WebAPI.Mapping.Core;

public sealed record TeamSingleDto : TeamDto
{
    public required List<PlayerBriefDto> Players { get; init; } = [];
}


