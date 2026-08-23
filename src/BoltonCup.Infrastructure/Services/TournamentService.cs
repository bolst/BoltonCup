using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

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