using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

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
        int draftId
    )
    {
        if (context.User.IsInRole(Admin))
        {
            context.Succeed(requirement);
            return;
        }
            
        if (!context.User.TryGetAccountId(out var accountId))
            return;
        
        var isTournamentGm = await _dbContext.Drafts
            .Where(d => d.Id == draftId)
            .Where(d => d.Tournament.Teams.Any(t => t.GmAccountId == accountId))
            .AnyAsync();
        
        if (isTournamentGm)
            context.Succeed(requirement);
    }
}