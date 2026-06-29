using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Auth;

public static partial class BoltonCupPolicy
{
    /// <summary>Policy that requires the user to own (or be an admin over) a specific custom ranking.</summary>
    public const string CanManageRanking = "CanManageRanking";
}

/// <summary>Authorization requirement for managing a custom ranking.</summary>
public class ManageRankingRequirement : IAuthorizationRequirement
{
}

/// <summary>Handles <see cref="ManageRankingRequirement"/> by verifying the user is an admin or the ranking's owner.</summary>
public class RankingManagerHandler(BoltonCupDbContext _dbContext)
    : AuthorizationHandler<ManageRankingRequirement>
{
    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageRankingRequirement requirement)
    {
        if (context.User.IsInRole(Admin))
        {
            context.Succeed(requirement);
            return;
        }

        if (!RankingAuthorizationResource.TryGetRankingId(context, out var rankingId))
            return;

        if (!context.User.TryGetAccountId(out var accountId))
            return;

        var isOwner = await _dbContext.CustomRankings
            .AnyAsync(r => r.Id == rankingId && r.AccountId == accountId);

        if (isOwner)
            context.Succeed(requirement);
    }
}
