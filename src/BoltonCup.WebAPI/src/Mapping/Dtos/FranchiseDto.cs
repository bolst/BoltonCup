namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a franchise in a list, with its title and season counts.</summary>
public sealed record FranchiseDto : FranchiseBriefDto
{
    /// <summary>Gets the number of tournaments won by any team of this franchise.</summary>
    public required int TitleCount { get; init; }
    /// <summary>Gets the number of seasons (team rows) this franchise has played.</summary>
    public required int SeasonCount { get; init; }
}