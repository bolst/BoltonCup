using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Auth;

public static partial class BoltonCupPolicy
{
    /// <summary>Policy that requires the user to be authorized to manage a specific draft.</summary>
    public const string CanManageDraft = "CanManageDraft";
}
/// <summary>Authorization requirement for managing a draft.</summary>
public class ManageDraftRequirement : IAuthorizationRequirement
{
}
/// <summary>Handles authorization for <see cref="ManageDraftRequirement"/> by verifying the user is an admin or the draft owner.</summary>
public class DraftManagerHandler(BoltonCupDbContext _dbContext)
    : AuthorizationHandler<ManageDraftRequirement, int>
{
    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageDraftRequirement requirement,
        int draftId
        )
    {
        if (context.User.IsInRole(Admin))
        {
            context.Succeed(requirement);
            return;
        }

        if (!context.User.TryGetAccountId(out var accountId))
        {
            return;
        }

        var isDraftOwner = await _dbContext.Drafts
            .AnyAsync(d => d.Id == draftId && d.DraftOwnerAccountId == accountId);

        if (isDraftOwner)
        {
            context.Succeed(requirement);
        }
    }
}