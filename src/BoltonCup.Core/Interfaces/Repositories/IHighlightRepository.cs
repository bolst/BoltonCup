namespace BoltonCup.Core;

public interface IHighlightRepository
{
    Task<IPagedList<Highlight>> GetAllAsync(GetHighlightsQuery query, CancellationToken cancellationToken = default);

    /// <summary>Gets the most recent highlights tagged with a given game.</summary>
    Task<IReadOnlyList<Highlight>> GetForGameAsync(int gameId, int take, CancellationToken cancellationToken = default);
}
