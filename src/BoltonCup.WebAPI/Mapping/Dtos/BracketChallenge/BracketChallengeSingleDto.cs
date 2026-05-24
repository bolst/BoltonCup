namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single bracket challenge, including terms of service.</summary>
public record BracketChallengeSingleDto : BracketChallengeDto
{
    /// <summary>Gets or sets the Markdown content of the terms of service.</summary>
    public string? TOSMarkdown { get; set; }
}
