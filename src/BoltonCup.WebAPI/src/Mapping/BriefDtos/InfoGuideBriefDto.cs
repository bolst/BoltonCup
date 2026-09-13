namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of an info guide.</summary>
public sealed record InfoGuideBriefDto
{
    /// <summary>Gets or sets the info guide title.</summary>
    public string? Title { get; set; }
    /// <summary>Gets or sets the Markdown content of the info guide.</summary>
    public string? MarkdownContent { get; set; }
}