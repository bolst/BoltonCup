using BoltonCup.Core;

namespace BoltonCup.Common.Services;

public class AssetUrlResolver(BoltonCupConfiguration configuration) 
    : IAssetUrlResolver
{
    private readonly string? _baseUrl = configuration.S3BaseUrl;

    public string? GetFullUrl(string? s3Key)
    {
        if (string.IsNullOrEmpty(s3Key)) 
            return null;
        return $"{_baseUrl}{s3Key}";
    }

    public HighlightUrls? GetHighlightUrls(string? videoId)
    {
        if (string.IsNullOrEmpty(videoId))
            return null;
        return new HighlightUrls(
            VideoUrl: $"https://www.youtube.com/embed/{videoId}",
            ThumbnailUrl: $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg"
        );
    }
}