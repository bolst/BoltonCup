using Microsoft.Extensions.Configuration;

namespace BoltonCup.Common.Services;

public interface IS3PathResolver
{
    string? GetFullUrl(string? s3Key);
}

public class PublicCdnUrlResolver : IS3PathResolver
{
    private readonly string? _baseUrl;

    public PublicCdnUrlResolver(IConfiguration config)
    {
        _baseUrl = config["CdnBaseUrl"];
    }

    public string? GetFullUrl(string? s3Key)
    {
        if (string.IsNullOrEmpty(s3Key)) 
            return null;
        return $"{_baseUrl}{s3Key}";
    }
}