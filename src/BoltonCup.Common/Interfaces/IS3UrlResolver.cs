namespace BoltonCup.Common;

public interface IS3UrlResolver
{
    string? GetFullUrl(string? s3Key);
}