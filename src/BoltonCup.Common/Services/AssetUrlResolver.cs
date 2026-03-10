namespace BoltonCup.Common.Services;

public class AssetUrlResolver : IAssetUrlResolver
{
    private readonly string? _baseUrl;

    public AssetUrlResolver(BoltonCupConfiguration configuration)
    {
        _baseUrl = configuration.S3BaseUrl;
    }

    public string? GetFullUrl(string? s3Key)
    {
        if (string.IsNullOrEmpty(s3Key)) 
            return null;
        return $"{_baseUrl}{s3Key}";
    }
}