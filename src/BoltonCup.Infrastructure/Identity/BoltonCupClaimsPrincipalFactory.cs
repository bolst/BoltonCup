using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BoltonCup.Infrastructure.Identity;

public class BoltonCupClaimsPrincipalFactory(
    UserManager<BoltonCupUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<BoltonCupUser, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BoltonCupUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        
        var accountIdStr = user.AccountId?.ToString();
        if (!string.IsNullOrEmpty(accountIdStr))
        {
            identity.AddClaim(new Claim(BoltonCupClaimTypes.AccountId, accountIdStr));
        }

        return identity;
    }
}

public static class BoltonCupClaimTypes
{
    public const string AccountId = "AccountId";
}