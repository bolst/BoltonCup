using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface ITeamRepository : IRepository<Team, GetTeamsQuery, int>
{
}