using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Models;

namespace BoltonCup.WebAPI.Repositories;

public class TeamRepository : ITeamRepository
{
    // private readonly ILogger<TeamRepository> _logger;
    private readonly string _connectionString;

    public TeamRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetBoltonCupConnectionString();
    }

    public Task<IEnumerable<Team>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Team?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddAsync(Team entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Team entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}