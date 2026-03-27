using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class AccountRepository(BoltonCupDbContext _context) : IAccountRepository
{
    public async Task<IPagedList<Account>> GetAllAsync(GetAccountsQuery query)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Players)
            .Include(a => a.ManagedTeams)
            .ConditionalWhere(a => a.Players.Any(p => p.TournamentId == query.TournamentId), query.TournamentId.HasValue)
            .ConditionalWhere(a => a.Players.Any(p => p.TeamId == query.TeamId), query.TeamId.HasValue)
            .ApplySorting(query, x => x.OrderBy(a => a.Id))
            .ToPagedListAsync(query);
    }
    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Players) .ThenInclude(p => p.Team)
            .Include(a => a.Players) .ThenInclude(p => p.Tournament)
            .Include(a => a.ManagedTeams)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}