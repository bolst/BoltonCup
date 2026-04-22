namespace BoltonCup.WebAPI.Mapping;

public record GalleryBriefDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
}