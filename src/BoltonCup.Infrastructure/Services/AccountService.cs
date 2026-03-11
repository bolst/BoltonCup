using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly BoltonCupDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IAssetKeyGenerator _assetKeyGenerator;

    public AccountService(BoltonCupDbContext dbContext, IStorageService storageService, IAssetKeyGenerator assetKeyGenerator)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _assetKeyGenerator = assetKeyGenerator;
    }
    
    public Task UpdateProfilePictureAsync(int accountId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _storageService.UpdateAssetAsync<Account>(
            _dbContext,
            _assetKeyGenerator,
            a => a.Id == accountId,
            (a, newKey) => a.ProfilePictureS3Key = newKey,
            tempKey,
            accountId.ToString(),
            "profile-picture",
            cancellationToken
        );
    }
}