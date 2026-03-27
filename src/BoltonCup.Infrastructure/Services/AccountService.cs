using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;

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

    public async Task<int> CreateAsync(CreateAccountCommand command, CancellationToken cancellationToken = default)
    {
        var newAccount = new Account
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            Birthday = command.Birthday,
            HighestLevel = command.HighestLevel,
            PreferredBeer = command.PreferredBeer,
        }; 
        _dbContext.Accounts.Add(newAccount);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return newAccount.Id;
    }

    public async Task UpdateAsync(UpdateAccountCommand command, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.Accounts.FindAsync([command.AccountId], cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Account not found.");

        account.FirstName = command.FirstName;
        account.LastName = command.LastName;
        account.Birthday = command.Birthday;
        account.HighestLevel = command.HighestLevel;
        account.PreferredBeer = command.PreferredBeer;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAvatarAsync(int accountId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _storageService.UpdateAssetAsync<Account>(
            _dbContext,
            _assetKeyGenerator,
            a => a.Id == accountId,
            a => a.Avatar,
            tempKey,
            accountId.ToString(),
            cancellationToken
        );
    }    
    
    public Task UpdateBannerAsync(int accountId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _storageService.UpdateAssetAsync<Account>(
            _dbContext,
            _assetKeyGenerator,
            a => a.Id == accountId,
            a => a.Banner,
            tempKey,
            accountId.ToString(),
            cancellationToken
        );
    }
}