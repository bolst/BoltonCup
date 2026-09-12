namespace BoltonCup.Core;

public interface IAssetKeyGenerator
{
    string GenerateTempKey(string fileExtension);
    string GenerateFinalKey<T>(string id, string asset, string extension);
    void ThrowIfNotValidTempKey(string key);
}