using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TournamentService : ITournamentService
{
    private readonly BoltonCupDbContext _dbContext;
    private readonly IAssetUploadService _assetUploadService;

    public TournamentService(BoltonCupDbContext dbContext, IAssetUploadService assetUploadService)
    {
        _dbContext = dbContext;
        _assetUploadService = assetUploadService;
    }
    
    public Task UpdateLogoAsync(int tournamentId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _assetUploadService.UpdateSingleAssetAsync<Tournament>(
            _dbContext,
            tempKey,
            x => x.Id == tournamentId,
            t => t.LogoS3Key,
            cancellationToken
        );
    }    
}