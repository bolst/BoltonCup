namespace BoltonCup.Shared;

public interface IAssetUrlResolver
{
    string? GetFullUrl(string? s3Key);
    HighlightUrls? GetHighlightUrls(string? videoId);
}

public sealed class HighlightUrls(string videoUrl, string thumbnailUrl)
{
    public string VideoUrl { get; } = videoUrl;

    public string ThumbnailUrl { get; } = thumbnailUrl;
}
