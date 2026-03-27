using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Extensions;

namespace BoltonCup.WebAPI.Mapping;

public interface IAccountMapper
{
    AccountDto? ToDto(Account? account, ClaimsPrincipal claims);
    ICollection<AccountTournamentDto> ToAccountTournamentDtoList(Account? account);
    CreateAccountCommand ToCommand(CompleteUserAccountRequest request, ClaimsPrincipal claims);
    UpdateAccountCommand ToCommand(UpdateAccountRequest request, ClaimsPrincipal claims);
}

public class AccountMapper(IBriefMapper _briefMapper) : IAccountMapper
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
            Phone = account.Phone ?? claims.FindFirstValue(ClaimTypes.MobilePhone),
            Birthday = account.Birthday,
            HighestLevel = account.HighestLevel,
            Avatar = account.Avatar,
            Banner = account.Banner,
            PreferredBeer = account.PreferredBeer,
        };
    }

    public ICollection<AccountTournamentDto> ToAccountTournamentDtoList(Account? account)
    {
        if (account is null)
            return [];
        return account.Players
            .Select(player => new AccountTournamentDto
            {
                Tournament = _briefMapper.ToTournamentBriefDto(player.Tournament),
                Team = player.Team == null ? null : _briefMapper.ToTeamBriefDto(player.Team)
            }) 
            .OrderByDescending(x => x.Tournament.StartDate)
            .ToList();
    }

    public CreateAccountCommand ToCommand(CompleteUserAccountRequest request, ClaimsPrincipal claims)
    {
        return new CreateAccountCommand(
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: claims.FindFirstValue(ClaimTypes.Email) ?? throw new InvalidOperationException("Missing email claim"),
            Birthday: request.Birthday,
            Height: $"{request.HeightFeet}'{request.HeightInches}\"",
            Weight: request.Weight,
            HighestLevel: request.HighestLevel,
            PreferredBeer: request.PreferredBeer
        );
    }

    public UpdateAccountCommand ToCommand(UpdateAccountRequest request, ClaimsPrincipal claims)
    {
        var accountId = claims.GetAccountId();
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
