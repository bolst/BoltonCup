namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single info guide, including its Markdown content.</summary>
public record InfoGuideSingleDto : InfoGuideDto
{
    /// <summary>Gets or sets the Markdown content of the info guide.</summary>
    public string? MarkdownContent { get; set; }
}

