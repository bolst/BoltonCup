using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;

namespace BoltonCup.WebAPI.Mapping;

public interface IAccountMapper
{
    AccountDto? ToDto(Account? account, ClaimsPrincipal claims);
    UpdateAccountCommand ToCommand(UpdateAccountRequest request, ClaimsPrincipal claims);
}

public class AccountMapper : IAccountMapper
{
    public AccountDto? ToDto(Account? account, ClaimsPrincipal claims)
    {
        if (account?.Id is null)
            return null;
        return new AccountDto
        {
            Id = account.Id,
            Email = account.Email,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Name = (account.FirstName +  " " + account.LastName).Trim(),
            IsAuthenticated = claims.Identity?.IsAuthenticated ?? false,
            Roles = claims.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
            Phone = account.Phone ?? claims.FindFirstValue(ClaimTypes.MobilePhone),
            Birthday = account.Birthday,
            HighestLevel = account.HighestLevel,
            Avatar = account.Avatar,
            Banner = account.Banner,
            PreferredBeer = account.PreferredBeer,
        };
    }

    public UpdateAccountCommand ToCommand(UpdateAccountRequest request, ClaimsPrincipal claims)
    {
        var accountIdStr = claims.FindFirstValue(ClaimTypes.Sid);
        if (int.TryParse(accountIdStr, out var accountId))
            throw new KeyNotFoundException("Missing account ID claim.");
        return new UpdateAccountCommand(
            AccountId: accountId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Birthday: request.Birthday,
            HighestLevel: request.HighestLevel,
            PreferredBeer: request.PreferredBeer
        );
    }
}
