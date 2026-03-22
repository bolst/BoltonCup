using System.Security.Claims;
using BoltonCup.Infrastructure.Identity;

namespace BoltonCup.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetAccountId(this ClaimsPrincipal principal)
    {
        var accountIdString = principal.FindFirstValue(BoltonCupClaimTypes.AccountId);
        if (int.TryParse(accountIdString, out var accountId))
            return accountId;
        return null;
    }
}