using System.Security.Claims;
using BoltonCup.Shared;

namespace BoltonCup.Draft;

public static class ClaimsPrincipalExtensions
{
    public static bool CanAccessDraft(this ClaimsPrincipal principal, Sdk.DraftDto draft)
    {
        return principal.IsGmForTournament(draft.Tournament.Id) || principal.IsInRole("Admin");
    }
    
    
    public static bool CanAccessDraft(this ClaimsPrincipal principal, Sdk.DraftSingleDto draft)
    {
        return principal.IsGmForTournament(draft.Tournament.Id) || principal.IsInRole("Admin");
    }

    public static bool CanManageDraft(this ClaimsPrincipal principal, Sdk.DraftSingleDto draft)
    {
        return draft.CanManageDraft;
    }
}