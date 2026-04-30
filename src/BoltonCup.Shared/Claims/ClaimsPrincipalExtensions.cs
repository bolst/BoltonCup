using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BoltonCup.Shared
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetAccountId(this ClaimsPrincipal principal)
        {
            var accountIdString = principal.FindFirst(claim => claim.Type == BoltonCupClaimTypes.AccountId)?.Value;
            return int.TryParse(accountIdString, out var accountId) 
                ? accountId 
                : throw new KeyNotFoundException("Missing account ID claim.");
        }

        public static bool TryGetAccountId(this ClaimsPrincipal principal, out int accountId)
        {
            var accountIdString = principal.FindFirst(claim => claim.Type == BoltonCupClaimTypes.AccountId)?.Value;
            return int.TryParse(accountIdString, out accountId);
        }

        public static int? GetAccountIdOrDefault(this ClaimsPrincipal principal)
        {
            if (principal.TryGetAccountId(out var accountId))
                return accountId;
            return null;
        }

        public static IEnumerable<int> GetTeamGmIds(this ClaimsPrincipal principal)
        {
            return principal
                .FindAll(claim => claim.Type == BoltonCupClaimTypes.TeamGm)
                .Select(claim => int.TryParse(claim.Value, out var n) ? n : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value);
        }

        public static IEnumerable<int> GetTournamentGmIds(this ClaimsPrincipal principal)
        {
            return principal
                .FindAll(claim => claim.Type == BoltonCupClaimTypes.TournamentGm)
                .Select(claim => int.TryParse(claim.Value, out var n) ? n : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value);
        }

        public static bool IsGmForTeam(this ClaimsPrincipal principal, int teamId) => 
            principal.HasClaim(claim => claim.Type == BoltonCupClaimTypes.TeamGm && claim.Value == teamId.ToString());

        public static bool IsGmForTournament(this ClaimsPrincipal principal, int tournamentId) => 
            principal.HasClaim(claim => claim.Type == BoltonCupClaimTypes.TournamentGm && claim.Value == tournamentId.ToString());
    }
}