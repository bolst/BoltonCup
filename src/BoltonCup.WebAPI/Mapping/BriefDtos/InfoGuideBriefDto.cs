namespace BoltonCup.WebAPI.Mapping;

public sealed record InfoGuideBriefDto
{
    public string? Title { get; set; }
    public string? MarkdownContent { get; set; }
}