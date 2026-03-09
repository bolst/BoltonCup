using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
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
    
    public async Task UpdateTeamLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        var team = await _dbContext.Teams.FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken: cancellationToken);
        if (team == null)
            throw new InvalidOperationException($"Team with ID {teamId} not found.");

        var command = new AssetCommitCommand<Team>
        {
            Entity = team,
            TempKey = tempKey,
            Destination = t => t.LogoS3Key
        };
        await _assetUploadService.CommitAsync(command, cancellationToken);
    }    
    
    public async Task UpdateTeamBannerAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        var team = await _dbContext.Teams.FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken: cancellationToken);
        if (team == null)
            throw new InvalidOperationException($"Team with ID {teamId} not found.");

        var command = new AssetCommitCommand<Team>
        {
            Entity = team,
            TempKey = tempKey,
            Destination = t => t.BannerS3Key
        };
        await _assetUploadService.CommitAsync(command, cancellationToken);
    }
}