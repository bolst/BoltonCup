namespace BoltonCup.Core;

public interface IInfoGuideRepository
{
    Task<IPagedList<InfoGuide>> GetAllAsync(GetInfoGuidesQuery query);
    Task<InfoGuide?> GetByIdAsync(Guid id);
    Task<InfoGuide?> GetByTournamentIdAsync(int tournamentId);
    Task<bool> AddAsync(InfoGuide entity);
    Task<bool> UpdateAsync(InfoGuide entity);
    Task<bool> DeleteAsync(Guid id);
}