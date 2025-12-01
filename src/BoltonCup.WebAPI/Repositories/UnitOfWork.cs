using BoltonCup.WebAPI.Interfaces;

namespace BoltonCup.WebAPI.Repositories;

public class UnitOfWork(ITeamRepository teams) : IUnitOfWork
{
    public ITeamRepository Teams { get; } = teams;
}
