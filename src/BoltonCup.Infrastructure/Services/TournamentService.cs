using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TournamentService : ITournamentService
{
    private readonly BoltonCupDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IAssetKeyGenerator _assetKeyGenerator;

    public TournamentService(BoltonCupDbContext dbContext, IStorageService storageService, IAssetKeyGenerator assetKeyGenerator)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _assetKeyGenerator = assetKeyGenerator;
    }
    
    public async Task UpdateLogoAsync(int tournamentId, string tempKey, CancellationToken cancellationToken = default)
    {
        var tournament = await _dbContext.Tournaments
                          .Where(a => a.Id == tournamentId)
                          .FirstOrDefaultAsync(cancellationToken) 
                      ?? throw new InvalidOperationException($"Tournament {tournamentId} does not exist");
        // commit asset to final location in S3
        var extension = Path.GetExtension(tempKey);
        var destination = _assetKeyGenerator.GenerateFinalKey<Tournament>(tournamentId.ToString(), "logo", extension);
        await _storageService.CopyAssetAsync(tempKey, destination, cancellationToken);
        // update tournament in db
        tournament.LogoS3Key = destination;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }    
}