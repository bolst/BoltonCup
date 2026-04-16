using BoltonCup.Core;

namespace BoltonCup.Infrastructure.Services;

public class AssetUrlResolver(string baseUrl) : IAssetUrlResolver
{
    private readonly string _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));

    public string? GetFullUrl(string? s3Key)
    {
        if (string.IsNullOrEmpty(s3Key)) 
            return null;
        return $"{_baseUrl}{s3Key}";
    }

    public string? GetVideoUrl(string? videoId)
    {
        if (string.IsNullOrEmpty(videoId))
            return null;
        return $"https://www.youtube.com/embed/{videoId}";
    }


    public static class StaticKeys
    {
        public const string PlayerAvatar = "static/defaults/player-avatar.webp";
        public const string PlayerBanner = "static/defaults/player-banner.jpg";
        public const string Logo = "static/branding/boltoncup.png";
    }
}