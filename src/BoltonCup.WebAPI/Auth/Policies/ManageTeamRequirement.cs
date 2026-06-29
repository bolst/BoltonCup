using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Auth;

public static partial class BoltonCupPolicy
{
    /// <summary>Policy that requires the user to be authorized to manage a specific team.</summary>
    public const string CanManageTeam = "CanManageTeam";
}
/// <summary>Authorization requirement for managing a team.</summary>
public class ManageTeamRequirement : IAuthorizationRequirement
{
}
/// <summary>Handles authorization for <see cref="ManageTeamRequirement"/> by verifying the user is an admin or the GM of the team.</summary>
public class TeamManagerHandler(BoltonCupDbContext _dbContext)
    : AuthorizationHandler<ManageTeamRequirement, int>
{
    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageTeamRequirement requirement,
        int teamId
        )
    {
        if (context.User.IsInRole(Admin))
        {
            context.Succeed(requirement);
            return;
        }

        if (!context.User.TryGetAccountId(out var accountId))
            return;

        var isTeamGm = await _dbContext.Teams
            .AnyAsync(t => t.Id == teamId && t.GmAccountId == accountId);

        if (isTeamGm)
            context.Succeed(requirement);
    }
}