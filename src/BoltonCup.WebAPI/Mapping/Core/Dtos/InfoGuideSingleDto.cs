namespace BoltonCup.WebAPI.Mapping.Core;

public record InfoGuideSingleDto : InfoGuideDto
{
    public string? MarkdownContent { get; set; }
}

