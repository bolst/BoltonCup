namespace BoltonCup.WebAPI.Mapping;

public record InfoGuideSingleDto : InfoGuideDto
{
    public string? MarkdownContent { get; set; }
}

