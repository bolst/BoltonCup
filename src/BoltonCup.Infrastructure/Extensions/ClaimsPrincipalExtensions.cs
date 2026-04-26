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

        public IEnumerable<int> GetTeamGmIds()
        {
            return principal
                .FindAll(claim => claim.Type == BoltonCupClaimTypes.TeamGm)
                .Select(claim => int.TryParse(claim.Value, out var n) ? n : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value);
        }

        public IEnumerable<int> GetTournamentGmIds()
        {
            return principal
                .FindAll(claim => claim.Type == BoltonCupClaimTypes.TournamentGm)
                .Select(claim => int.TryParse(claim.Value, out var n) ? n : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value);
        }
    }
}