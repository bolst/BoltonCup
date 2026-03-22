namespace BoltonCup.WebAPI.Mapping;

public sealed record GameSingleDto : GameDto
{
    public required List<GoalBriefDto> Goals { get; init; } = [];
    public required List<PenaltyBriefDto> Penalties { get; init; } = [];
}


