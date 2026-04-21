using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.WebAPI.Auth;

public static partial class BoltonCupPolicy
{
    public const string CanAccessDraft = "CanAccessDraft";
}

public class AccessDraftRequirement : IAuthorizationRequirement
{
}

public class DraftAccessHandler(BoltonCupDbContext _dbContext) 
    : AuthorizationHandler<AccessDraftRequirement, int>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        AccessDraftRequirement requirement, 
        int tournamentId
    )
    {
        if (!context.User.TryGetAccountId(out var accountId))
            return;
        
        var isTournamentGm = await _dbContext.Teams
            .Where(t => t.TournamentId == tournamentId)
            .AnyAsync(t => t.GmAccountId == accountId);
        
        if (isTournamentGm)
            context.Succeed(requirement);
    }
}