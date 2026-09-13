using BoltonCup.Core;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Authorization;
using static BoltonCup.Shared.BoltonCupRole;

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
public class DraftManagerHandler(IDraftService _drafts)
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

        var isDraftOwner = await _drafts.CanManageAsync(draftId, accountId);

        if (isDraftOwner)
        {
            context.Succeed(requirement);
        }
    }
}