namespace BoltonCup.Core;

public interface IGameHighlightRepository
{
    Task<IPagedList<GameHighlight>> GetAllAsync(GetHighlightsQuery query, CancellationToken cancellationToken = default);
}
