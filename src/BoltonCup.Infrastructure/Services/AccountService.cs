using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly BoltonCupDbContext _dbContext;
    private readonly IAssetUploadService _assetUploadService;
    private readonly IAssetKeyGenerator _assetKeyGenerator;

    public AccountService(BoltonCupDbContext dbContext, IAssetUploadService assetUploadService, IAssetKeyGenerator assetKeyGenerator)
    {
        _dbContext = dbContext;
        _assetUploadService = assetUploadService;
        _assetKeyGenerator = assetKeyGenerator;
    }
    
    public async Task UpdateProfilePictureAsync(int accountId, string tempKey, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.Accounts
                       .Where(a => a.Id == accountId)
                       .FirstOrDefaultAsync(cancellationToken) 
                   ?? throw new InvalidOperationException($"Account {accountId} does not exist");
        // commit asset to final location in S3
        var extension = Path.GetExtension(tempKey);
        var destination = _assetKeyGenerator.GenerateFinalKey<Account>(accountId.ToString(), "profile-picture", extension);
        await _assetUploadService.CopyAssetAsync(tempKey, destination, cancellationToken);
        // update account in db
        account.ProfilePictureS3Key = destination;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}