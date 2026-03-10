using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
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
    
    public async Task UpdateLogoAsync(int tournamentId, string tempKey, CancellationToken cancellationToken = default)
    {
        var tournament = await _dbContext.Tournaments.FirstOrDefaultAsync(x => x.Id == tournamentId, cancellationToken: cancellationToken);
        if (tournament == null)
            throw new InvalidOperationException($"Tournament with ID {tournamentId} not found.");

        var command = new AssetCommitCommand<Tournament>
        {
            Entity = tournament,
            TempKey = tempKey,
            Destination = t => t.LogoS3Key
        };
        await _assetUploadService.CommitAsync(command, cancellationToken);
    }    
}