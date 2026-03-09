using BoltonCup.Core.Interfaces.Base;
using BoltonCup.Core.Mappings;

namespace BoltonCup.Core;

public interface IInfoGuideRepository : IRepository<InfoGuide, GetInfoGuidesQuery, Guid>
{
    Task<InfoGuide?> GetByTournamentIdAsync(int tournamentId);
    Task<TResult?> GetByTournamentIdAsync<TResult>(int tournamentId)
        where TResult : IMappable<InfoGuide, TResult>;
}