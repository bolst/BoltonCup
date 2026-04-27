using BoltonCup.Sdk;

namespace BoltonCup.Draft.Services;


public interface IDraftStateServiceFactory
{
    Task<DraftStateService?> CreateAsync(int draftId);
}

public class DraftStateServiceFactory : IDraftStateServiceFactory
{
    private readonly IBoltonCupApi _api;
    
    private DraftStateServiceFactory(IBoltonCupApi api)
    {
        _api = api;
    }

    public async Task<DraftStateService?> CreateAsync(int draftId)
    {
        try
        {
            return await DraftStateService.CreateAsync(draftId, _api);
        }
        catch
        {
            return null;
        }
    }
    
    
}