namespace BoltonCup.Core;

public interface IAssetUrlResolver
{
    string? GetFullUrl(string? s3Key);
    string? GetVideoUrl(string? videoId);
}