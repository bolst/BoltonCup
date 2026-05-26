namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of a gallery item.</summary>
public record GalleryBriefDto
{
    /// <summary>Gets or sets the gallery item ID.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the gallery item title.</summary>
    public string? Title { get; set; }
    /// <summary>Gets or sets the gallery item description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the URL of the gallery media.</summary>
    public string? Url { get; set; }
}