namespace BoltonCup.Core;

public interface IGeneratedImageService
{
    Task<GeneratedImage> CreateAsync(string storageKey, string templateKey, string label, string contentType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GeneratedImage>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default);
}