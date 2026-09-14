namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single news post, including its Markdown content.</summary>
public record NewsPostSingleDto : NewsPostDto
{
    /// <summary>Gets the Markdown body of the post.</summary>
    public string? MarkdownContent { get; init; }
}
