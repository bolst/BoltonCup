using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
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
    
    public Task UpdateProfilePictureAsync(int accountId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _assetUploadService.UpdateSingleAssetAsync<Account>(
            _dbContext,
            tempKey,
            x => x.Id == accountId,
            t => t.ProfilePictureS3Key,
            cancellationToken
        );
    }
}