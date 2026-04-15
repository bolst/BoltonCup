namespace BoltonCup.WebAPI.Mapping;

public record BracketChallengeSingleDto : BracketChallengeDto
{
    public string? TOSMarkdown { get; set; }
}
