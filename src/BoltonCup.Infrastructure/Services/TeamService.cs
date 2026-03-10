using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly BoltonCupDbContext _dbContext;
    private readonly IAssetUploadService _assetUploadService;

    public TeamService(BoltonCupDbContext dbContext, IAssetUploadService assetUploadService)
    {
        _dbContext = dbContext;
        _assetUploadService = assetUploadService;
    }
    
    public Task UpdateLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _assetUploadService.UpdateSingleAssetAsync<Team>(
            _dbContext,
            tempKey,
            x => x.Id == teamId,
            t => t.LogoS3Key,
            cancellationToken
        );
    }    
    
    public Task UpdateBannerAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _assetUploadService.UpdateSingleAssetAsync<Team>(
            _dbContext,
            tempKey,
            x => x.Id == teamId,
            t => t.BannerS3Key,
            cancellationToken
        );
    }
}