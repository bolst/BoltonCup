using BoltonCup.Sdk;

namespace BoltonCup.WebClient.Services;

public class BcStateService(IBoltonCupApi _api)
{
    private SystemContextDto? _context;
    
    public Task<SystemContextDto> Context => GetContextAsync();

    private async Task<SystemContextDto> GetContextAsync()
    {
        return _context ??= await _api.GetSystemContextAsync();
    }
}