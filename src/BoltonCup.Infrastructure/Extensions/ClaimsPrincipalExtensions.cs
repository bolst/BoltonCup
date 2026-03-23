using System.Security.Claims;
using BoltonCup.Infrastructure.Identity;

namespace BoltonCup.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetAccountId(this ClaimsPrincipal principal)
    {
        var accountIdString = principal.FindFirstValue(BoltonCupClaimTypes.AccountId);
        return int.TryParse(accountIdString, out var accountId) 
            ? accountId 
            : throw new KeyNotFoundException("Missing account ID claim.");
    }

    public static bool TryGetAccountId(this ClaimsPrincipal principal, out int accountId)
    {
        var accountIdString = principal.FindFirstValue(BoltonCupClaimTypes.AccountId);
        return int.TryParse(accountIdString, out accountId);
    }
}