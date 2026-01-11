using BoltonCup.Core.Base;

namespace BoltonCup.Core;

public interface ITeamRepository : IRepository<Team, GetTeamsQuery, int>
{
}