using System.Security.Claims;
using BoltonCup.Infrastructure.Identity;

namespace BoltonCup.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public int GetAccountId()
        {
            var accountIdString = principal.FindFirstValue(BoltonCupClaimTypes.AccountId);
            return int.TryParse(accountIdString, out var accountId) 
                ? accountId 
                : throw new KeyNotFoundException("Missing account ID claim.");
        }

        public bool TryGetAccountId(out int accountId)
        {
            var accountIdString = principal.FindFirstValue(BoltonCupClaimTypes.AccountId);
            return int.TryParse(accountIdString, out accountId);
        }

        public int? GetAccountIdOrDefault()
        {
            if (principal.TryGetAccountId(out var accountId))
                return accountId;
            return null;
        }
    }
}