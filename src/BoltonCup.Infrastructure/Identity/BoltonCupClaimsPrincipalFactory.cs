using System.Security.Claims;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BoltonCup.Infrastructure.Identity;

public class BoltonCupClaimsPrincipalFactory(
    UserManager<BoltonCupUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor,
    BoltonCupDbContext dbContext)
    : UserClaimsPrincipalFactory<BoltonCupUser, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BoltonCupUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        
        var accountIdStr = user.AccountId?.ToString();
        if (!string.IsNullOrEmpty(accountIdStr))
        {
            identity.AddClaim(new Claim(BoltonCupClaimTypes.AccountId, accountIdStr));
            
            // add team GM claim(s)
            var gmTournaments = await dbContext.Teams
                .Where(t => t.GeneralManagers.Any(g => g.Id == user.AccountId))
                .GroupBy(t => t.TournamentId)
                .ToListAsync();
            foreach (var tournament in gmTournaments)
            {
                if (tournament.Key is { } tournamentId)
                {
                    identity.AddClaim(new Claim(BoltonCupClaimTypes.TournamentGm, tournamentId.ToString()));
                }
                
                identity.AddClaims(
                    tournament.Select(team => new Claim(BoltonCupClaimTypes.TeamGm, team.Id.ToString())).ToArray()
                );
            }
        }

        return identity;
    }
}