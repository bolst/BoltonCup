using BoltonCup.Sdk;

namespace BoltonCup.Draft.Services;


public class DraftStateService : IAsyncDisposable
{
    private readonly IBoltonCupApi _api;
    private readonly DraftSingleDto _draft;

    public DraftSingleDto Draft => _draft;
    
    private DraftStateService(DraftSingleDto draft, IBoltonCupApi api)
    {
        _draft = draft;
        _api = api;
    }

    public static async Task<DraftStateService> CreateAsync(int draftId, IBoltonCupApi api)
    {
        var draft = await api.GetDraftByIdAsync(draftId)
                    ?? throw new InvalidOperationException($"No draft with ID {draftId} exists.");

        return new DraftStateService(draft, api);
    }

    public async ValueTask DisposeAsync()
    {
        
    }
}