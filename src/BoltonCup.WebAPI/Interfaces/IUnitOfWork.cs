namespace BoltonCup.WebAPI.Interfaces;

public interface IUnitOfWork
{
    ITeamRepository Teams { get; }
}
