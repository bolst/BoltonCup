using BoltonCup.Sdk;

namespace BoltonCup.WebClient.Services;

public class TournamentStateService(IBoltonCupApi _api)
{
    private SystemContextDto? _context;
    private TournamentSingleDto? _tournament;

    private async Task<SystemContextDto> GetContextAsync()
    {
        return _context ??= await _api.GetSystemContextAsync();
    }
    
    public async Task<TournamentSingleDto?> GetActiveTournamentAsync()
    {
        var context = await GetContextAsync();
        if (context.ActiveTournamentId is not {} activeTournamentId) 
            return null;
        return _tournament ??= await _api.GetTournamentByIdAsync(activeTournamentId);
    }
}