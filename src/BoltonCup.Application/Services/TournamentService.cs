using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

public class TournamentService : ITournamentService
{
    readonly BoltonCupDbContext _dbContext;
    readonly IStorageService _storageService;
    readonly IAssetKeyGenerator _assetKeyGenerator;

    public TournamentService(BoltonCupDbContext dbContext, IStorageService storageService, IAssetKeyGenerator assetKeyGenerator)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _assetKeyGenerator = assetKeyGenerator;
    }

    public async Task<IPagedList<Tournament>> GetAllAsync(GetTournamentsQuery query, CancellationToken cancellationToken = default) => await _dbContext.Tournaments
            .AsNoTracking()
            .ConditionalWhere(e => e.IsRegistrationOpen == query.RegistrationOpen!.Value, query.RegistrationOpen.HasValue)
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .Include(e => e.Gallery)
            .ApplySorting(query, x => x.OrderBy(t => t.StartDate))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);

    public async Task<Tournament?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await _dbContext.Tournaments
            .AsNoTracking()
            .Include(e => e.InfoGuide)
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .Include(e => e.Gallery)
            .Include(e => e.Sponsors
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortKey)
            )
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken: cancellationToken);

    public async Task<Tournament?> GetActiveAsync(CancellationToken cancellationToken = default) => await _dbContext.Tournaments
            .AsNoTracking()
            .Include(e => e.InfoGuide)
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .Include(e => e.Gallery)
            .Include(e => e.Sponsors
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortKey)
            )
            .FirstOrDefaultAsync(e => e.IsActive, cancellationToken: cancellationToken);

    public Task UpdateLogoAsync(int tournamentId, string tempKey, CancellationToken cancellationToken = default) => _storageService.UpdateAssetAsync<Tournament>(
            _dbContext,
            _assetKeyGenerator,
            t => t.Id == tournamentId,
            t => t.Logo,
            tempKey,
            tournamentId.ToString(),
            cancellationToken
        );

    public Task UpdateBackgroundImageAsync(int tournamentId, string tempKey, CancellationToken cancellationToken = default) => _storageService.UpdateAssetAsync<Tournament>(
            _dbContext,
            _assetKeyGenerator,
            t => t.Id == tournamentId,
            t => t.BackgroundImage,
            tempKey,
            tournamentId.ToString(),
            cancellationToken
        );
}