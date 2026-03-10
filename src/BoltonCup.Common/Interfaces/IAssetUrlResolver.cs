namespace BoltonCup.Common;

public interface IAssetUrlResolver
{
    string? GetFullUrl(string? s3Key);
}