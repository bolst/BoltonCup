using System.Security.Claims;
using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping.Core;

public interface IAccountMapper
{
    AccountDto? ToDto(Account? account, ClaimsPrincipal claims);
}

public class AccountMapper : IAccountMapper
{
    public AccountDto? ToDto(Account? account, ClaimsPrincipal claims)
    {
        return new AccountDto
        {
            Id = account?.Id,
            Email = account?.Email ?? claims.FindFirstValue(ClaimTypes.Email),
            FirstName = account?.FirstName,
            LastName = account?.LastName,
            Name = (account?.FirstName +  " " + account?.LastName).Trim(),
            IsAuthenticated = claims.Identity?.IsAuthenticated ?? false,
            Roles = claims.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
            Phone = account?.Phone ?? claims.FindFirstValue(ClaimTypes.MobilePhone),
            Birthday = account?.Birthday,
            HighestLevel = account?.HighestLevel,
            Avatar = account?.Avatar,
            Banner = account?.Banner,
            PreferredBeer = account?.PreferredBeer,
        };
    }
}
