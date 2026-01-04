namespace BoltonCup.Core;

public interface ITournamentRepository : IRepository<Tournament, int>
{
    Task<IEnumerable<Tournament>> GetAllAsync();
    Task<Tournament?> GetActiveAsync();
}