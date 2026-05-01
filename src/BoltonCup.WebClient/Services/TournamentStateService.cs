using BoltonCup.Sdk;

namespace BoltonCup.WebClient.Services;

public class TournamentStateService(IBoltonCupApi _api)
{
    private TournamentSingleDto? _tournament;
    
    public async Task<TournamentSingleDto?> GetActiveTournamentAsync()
    {
        return _tournament ??= await _api.GetActiveTournamentAsync();
    }
}