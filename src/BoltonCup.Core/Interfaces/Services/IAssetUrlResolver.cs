namespace BoltonCup.Core;

public interface IAssetUrlResolver
{
    string? GetFullUrl(string? s3Key);
    HighlightUrls? GetHighlightUrls(string? videoId);
}

public record HighlightUrls(string VideoUrl, string ThumbnailUrl);