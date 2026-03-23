namespace BoltonCup.WebAPI.Mapping;

public sealed record TournamentSingleDto : TournamentDto
{
    public InfoGuideBriefDto? InfoGuide { get; init; }
}


