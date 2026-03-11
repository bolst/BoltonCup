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
    
    public async Task UpdateLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        var team = await _dbContext.Teams
            .Where(t => t.Id == teamId)
            .FirstOrDefaultAsync(cancellationToken) 
                   ?? throw new InvalidOperationException($"Team {teamId} does not exist");
        // commit asset to final location in S3
        var extension = Path.GetExtension(tempKey);
        var destination = _assetKeyGenerator.GenerateFinalKey<Team>(teamId.ToString(), "logo", extension);
        await _storageService.CopyAssetAsync(tempKey, destination, cancellationToken);
        // update team in db
        team.LogoS3Key = destination;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }    
    
    public async Task UpdateBannerAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        var team = await _dbContext.Teams
                       .Where(t => t.Id == teamId)
                       .FirstOrDefaultAsync(cancellationToken) 
                   ?? throw new InvalidOperationException($"Team {teamId} does not exist");
        // commit asset to final location in S3
        var extension = Path.GetExtension(tempKey);
        var destination = _assetKeyGenerator.GenerateFinalKey<Team>(teamId.ToString(), "banner", extension);
        await _storageService.CopyAssetAsync(tempKey, destination, cancellationToken);
        // update team in db
        team.BannerS3Key = destination;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}