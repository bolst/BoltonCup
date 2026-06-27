using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

/// <summary>A user an admin can masquerade as.</summary>
public record MasqueradeUser(string UserId, int? AccountId, string Name, string Email);

public interface IMasqueradeService
{
    Task<IReadOnlyList<MasqueradeUser>> SearchAsync(string? query, int limit = 5, CancellationToken cancellationToken = default);
}

public class MasqueradeService(
    BoltonCupDbContext _dbContext,
    UserManager<BoltonCupUser> _userManager) : IMasqueradeService
{
    public async Task<IReadOnlyList<MasqueradeUser>> SearchAsync(string? query, int limit = 5, CancellationToken cancellationToken = default)
    {
        query = query?.Trim();
        var hasQuery = !string.IsNullOrEmpty(query);
        var pattern = $"%{query}%";

        // Match accounts (which hold names) by name or email.
        var accountsQuery = _dbContext.Accounts.AsNoTracking();
        if (hasQuery)
        {
            accountsQuery = accountsQuery.Where(a =>
                EF.Functions.ILike(a.FirstName + " " + a.LastName, pattern)
                || EF.Functions.ILike(a.Email, pattern));
        }

        var accountById = await accountsQuery
            .OrderBy(a => a.FirstName)
            .ThenBy(a => a.LastName)
            .Take(limit)
            .Select(a => new { a.Id, a.FirstName, a.LastName, a.Email })
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        // Identity users linked to the matched accounts (only users can be masqueraded as).
        var usersByAccount = await _userManager.Users
            .Where(u => u.AccountId != null && accountById.Keys.Contains(u.AccountId.Value))
            .Select(u => new { u.Id, u.AccountId, u.Email })
            .ToListAsync(cancellationToken);

        var results = new Dictionary<string, MasqueradeUser>();
        foreach (var user in usersByAccount)
        {
            var account = accountById[user.AccountId!.Value];
            var name = $"{account.FirstName} {account.LastName}".Trim();
            var email = account.Email;
            results[user.Id] = new MasqueradeUser(user.Id, user.AccountId, string.IsNullOrEmpty(name) ? email : name, email);
        }

        return results.Values
            .OrderBy(r => r.Name)
            .Take(limit)
            .ToList();
    }
}
