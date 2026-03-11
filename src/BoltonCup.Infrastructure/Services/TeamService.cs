using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly BoltonCupDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IAssetKeyGenerator _assetKeyGenerator;

    public TeamService(BoltonCupDbContext dbContext, IStorageService storageService, IAssetKeyGenerator assetKeyGenerator)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _assetKeyGenerator = assetKeyGenerator;
    }
    
    public Task UpdateLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _storageService.UpdateAssetAsync<Team>(
            _dbContext,
            _assetKeyGenerator,
            t => t.Id == teamId,
            (t, newKey) => t.LogoS3Key = newKey,
            tempKey,
            teamId.ToString(),
            "logo",
            cancellationToken
        );
    }    
    
    public Task UpdateBannerAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _storageService.UpdateAssetAsync<Team>(
            _dbContext,
            _assetKeyGenerator,
            t => t.Id == teamId,
            (t, newKey) => t.BannerS3Key = newKey,
            tempKey,
            teamId.ToString(),
            "banner",
            cancellationToken
        );
    }
}