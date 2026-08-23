using System.Security.Claims;
using BoltonCup.Shared;

namespace BoltonCup.Draft;

public static class ClaimsPrincipalExtensions
{
    public static bool CanAccessDraft(this ClaimsPrincipal principal, Sdk.DraftDto draft) => principal.IsGmForTournament(draft.Tournament.Id) || principal.IsInRole("Admin");

    public static bool CanAccessDraft(this ClaimsPrincipal principal, Sdk.DraftSingleDto draft) => principal.IsGmForTournament(draft.Tournament.Id) || principal.IsInRole("Admin");

    public static bool CanManageDraft(this ClaimsPrincipal principal, Sdk.DraftSingleDto draft) => draft.CanManageDraft;
}