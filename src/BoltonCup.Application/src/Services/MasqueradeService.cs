using BoltonCup.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

/// <summary>A user an admin can masquerade as.</summary>
public record MasqueradeUser(string UserId, int? AccountId, string Name, string Email);

public interface IMasqueradeService
{
    Task<IReadOnlyList<MasqueradeUser>> SearchAsync(string? query, int limit = 5, CancellationToken cancellationToken = default);
}

class MasqueradeService(BoltonCupDbContext _dbContext) : IMasqueradeService
{
    public async Task<IReadOnlyList<MasqueradeUser>> SearchAsync(string? query, int limit = 5, CancellationToken cancellationToken = default)
    {
        query = query?.Trim();
        var hasQuery = !string.IsNullOrEmpty(query);
        var pattern = $"%{query}%";

        // Match accounts (which hold names) by name or email. Only accounts a user owns can be masqueraded as.
        var accountsQuery = _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.UserId != null);
        if (hasQuery)
        {
            accountsQuery = accountsQuery.Where(a =>
                EF.Functions.ILike(a.FirstName + " " + a.LastName, pattern)
                || EF.Functions.ILike(a.Email, pattern));
        }

        var accounts = await accountsQuery
            .OrderBy(a => a.FirstName)
            .ThenBy(a => a.LastName)
            .Take(limit)
            .Select(a => new { a.Id, a.UserId, a.FirstName, a.LastName, a.Email })
            .ToListAsync(cancellationToken);

        return accounts.Select(account =>
            {
                var name = $"{account.FirstName} {account.LastName}".Trim();
                return new MasqueradeUser(account.UserId!, account.Id, string.IsNullOrEmpty(name) ? account.Email : name, account.Email);
            })
            .ToList();
    }
}