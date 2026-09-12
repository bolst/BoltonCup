using BoltonCup.Core;
using BoltonCup.Application.Extensions;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Authorization;
using static BoltonCup.Persistence.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Auth;

public static partial class BoltonCupPolicy
{
    /// <summary>Policy that requires the user to be authorized to access a specific draft.</summary>
    public const string CanAccessDraft = "CanAccessDraft";
}
/// <summary>Authorization requirement for accessing a draft.</summary>
public class AccessDraftRequirement : IAuthorizationRequirement
{
}
/// <summary>Handles authorization for <see cref="AccessDraftRequirement"/> by verifying the user is an admin or a GM of the draft's tournament.</summary>
public class DraftAccessHandler(IDraftService _drafts)
    : AuthorizationHandler<AccessDraftRequirement, int>
{
    /// <inheritdoc/>
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
        {
            return;
        }

        var isTournamentGm = await _drafts.CanAccessAsync(draftId, accountId);

        if (isTournamentGm)
        {
            context.Succeed(requirement);
        }
    }
}