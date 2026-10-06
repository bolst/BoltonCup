namespace BoltonCup.Core;

/// <summary>
/// Copies an existing asset into a fresh temp key, server-side. Kept separate from
/// <see cref="IStorageService"/> so the WASM client storage service need not implement a
/// server-only operation.
/// </summary>
public interface IAssetStager
{
    /// <summary>Copies the asset at <paramref name="sourceKey"/> to a new temp key and returns it.</summary>
    Task<string> CopyToTempAsync(string sourceKey, CancellationToken cancellationToken = default);
}