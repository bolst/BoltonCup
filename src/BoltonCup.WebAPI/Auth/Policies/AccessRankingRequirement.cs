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
    /// <summary>Policy that requires the user to be able to view a custom ranking (owner, admin, or shared viewer).</summary>
    public const string CanAccessRanking = "CanAccessRanking";
}
/// <summary>Resolves the ranking id under authorization from the request route or an explicit int resource.</summary>
internal static class RankingAuthorizationResource
{
    public static bool TryGetRankingId(AuthorizationHandlerContext context, out int rankingId)
    {
        switch (context.Resource)
        {
            case int id:
                rankingId = id;
                return true;
            case HttpContext http
                when http.Request.RouteValues.TryGetValue("id", out var value)
                     && int.TryParse(value?.ToString(), out var parsed):
                rankingId = parsed;
                return true;
            default:
                rankingId = 0;
                return false;
        }
    }
}
/// <summary>Authorization requirement for viewing a custom ranking.</summary>
public class AccessRankingRequirement : IAuthorizationRequirement
{
}
/// <summary>Handles <see cref="AccessRankingRequirement"/> by verifying the user is an admin, the owner, or a shared viewer.</summary>
public class RankingAccessHandler(BoltonCupDbContext _dbContext)
    : AuthorizationHandler<AccessRankingRequirement>
{
    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccessRankingRequirement requirement)
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

        var canView = await _dbContext.CustomRankings
            .AnyAsync(r => r.Id == rankingId
                           && (r.AccountId == accountId
                               || r.SharedWith.Any(s => s.SharedWithAccountId == accountId)));

        if (canView)
            context.Succeed(requirement);
    }
}