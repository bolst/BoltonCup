using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Core.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class AccountRepository(BoltonCupDbContext _context) : IAccountRepository
{
    public async Task<CollectionResult<Account>> GetAllAsync(GetAccountsQuery query)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Players)
            .Include(a => a.ManagedTeams)
            .ConditionalWhere(a => a.Players.Any(p => p.TournamentId == query.TournamentId), query.TournamentId.HasValue)
            .ConditionalWhere(a => a.Players.Any(p => p.TeamId == query.TeamId), query.TeamId.HasValue)
            .OrderBy(a => a.Id)
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetAccountsQuery query)
        where T : IMappable<Account, T>
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Players)
            .Include(a => a.ManagedTeams)
            .ConditionalWhere(a => a.Players.Any(p => p.TournamentId == query.TournamentId), query.TournamentId.HasValue)
            .ConditionalWhere(a => a.Players.Any(p => p.TeamId == query.TeamId), query.TeamId.HasValue)
            .OrderBy(a => a.Id)
            .ProjectTo<Account, T>()
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Players)
            .Include(a => a.ManagedTeams)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<Account, T>
    {
        return await _context.Accounts
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<Account, T>(a => a.Id == id);
    }

    public async Task<bool> AddAsync(Account entity)
    {
        await _context.Accounts.AddAsync(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Account entity)
    {
        _context.Accounts.Update(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Accounts.FindAsync(id);
        if (entity == null) return false;

        _context.Accounts.Remove(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}