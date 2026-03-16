using BoltonCup.Core;

namespace BoltonCup.Infrastructure.Services;

public class AssetKeyGenerator : IAssetKeyGenerator
{
    private const string _tempKeyPrefix = "temp_uploads/";
    
    public string GenerateTempKey(string fileExtension)
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        return $"{_tempKeyPrefix}{uniqueSuffix}{fileExtension}";
    }
    
    public string GenerateFinalKey<T>(string id, string asset, string extension)
    {
        var type = typeof(T).Name.ToLower();
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        return $"media/{type}/{id}/{asset}/{uniqueSuffix}{extension}";
    }

    public void ThrowIfNotValidTempKey(string key)
    {
        if (string.IsNullOrEmpty(key) || !key.StartsWith(_tempKeyPrefix))
            throw new ArgumentException("Invalid temporary key.");
    }
}