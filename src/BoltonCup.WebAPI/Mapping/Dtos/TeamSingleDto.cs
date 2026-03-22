namespace BoltonCup.WebAPI.Mapping;

public sealed record TeamSingleDto : TeamDto
{
    public required List<PlayerBriefDto> Players { get; init; } = [];
}


