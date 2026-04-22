using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.WebAPI.Auth;

public static partial class BoltonCupPolicy
{
    public const string CanManageTeam = "CanManageTeam";
}

public class ManageTeamRequirement : IAuthorizationRequirement
{
}

public class TeamManagerHandler(BoltonCupDbContext _dbContext) 
    : AuthorizationHandler<ManageTeamRequirement, int>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ManageTeamRequirement requirement, 
        int teamId
    )
    {
        if (!context.User.TryGetAccountId(out var accountId))
            return;
        
        var isTeamGm = await _dbContext.Teams
            .AnyAsync(t => t.Id == teamId && t.GmAccountId == accountId);
        
        if (isTeamGm)
            context.Succeed(requirement);
    }
}