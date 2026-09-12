using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

class AccountService : IAccountService
{
    readonly BoltonCupDbContext _dbContext;
    readonly IStorageService _storageService;
    readonly IAssetKeyGenerator _assetKeyGenerator;

    public AccountService(BoltonCupDbContext dbContext, IStorageService storageService, IAssetKeyGenerator assetKeyGenerator)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _assetKeyGenerator = assetKeyGenerator;
    }

    public async Task<IPagedList<Account>> GetAllAsync(GetAccountsQuery query, CancellationToken cancellationToken = default) => await _dbContext.Accounts
            .AsNoTracking()
            .Include(a => a.Players)
            .Include(a => a.ManagedTeams)
            .ConditionalWhere(a => a.Players.Any(p => p.TournamentId == query.TournamentId), query.TournamentId.HasValue)
            .ConditionalWhere(a => a.Players.Any(p => p.TeamId == query.TeamId), query.TeamId.HasValue)
            .ApplySorting(query, x => x.OrderBy(a => a.Id))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);

    public async Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await _dbContext.Accounts
            .AsNoTracking()
            .Include(a => a.Players).ThenInclude(p => p.Team)
            .Include(a => a.Players).ThenInclude(p => p.Tournament)
            .Include(a => a.ManagedTeams)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken: cancellationToken);

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
            HeightFeet = command.HeightFeet,
            HeightInches = command.HeightInches,
            Weight = command.Weight,
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
        account.HeightFeet = command.HeightFeet;
        account.HeightInches = command.HeightInches;
        account.Weight = command.Weight;

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

    public Task UpdatePreviousTeamLogoAsync(int accountId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _storageService.UpdateAssetAsync<Account>(
            _dbContext,
            _assetKeyGenerator,
            a => a.Id == accountId,
            a => a.PreviousTeamLogo,
            tempKey,
            accountId.ToString(),
            cancellationToken
        );
    }
}