using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly BoltonCupDbContext _dbContext;
    private readonly IAssetUploadService _assetUploadService;

    public AccountService(BoltonCupDbContext dbContext, IAssetUploadService assetUploadService)
    {
        _dbContext = dbContext;
        _assetUploadService = assetUploadService;
    }
    
    public async Task UpdateProfilePictureAsync(int accountId, string tempKey, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken: cancellationToken);
        if (account == null)
            throw new InvalidOperationException($"Tournament with ID {accountId} not found.");

        var command = new AssetCommitCommand<Account>
        {
            Entity = account,
            TempKey = tempKey,
            Destination = t => t.ProfilePictureS3Key
        };
        await _assetUploadService.CommitAsync(command, cancellationToken);
    }    
}