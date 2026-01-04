namespace BoltonCup.Core;

public interface ITournamentRepository : IRepository<Tournament, int>
{
    Task<IEnumerable<Tournament>> GetAllAsync(GetTournamentsQuery query);
    Task<Tournament?> GetActiveAsync();
}