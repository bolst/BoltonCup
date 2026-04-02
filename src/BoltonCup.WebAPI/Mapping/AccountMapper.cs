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
            HeightFeet = account.HeightFeet,
            HeightInches = account.HeightInches,
            Weight = account.Weight
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
            HeightFeet: request.HeightFeet,
            HeightInches: request.HeightInches,
            Weight: request.Weight,
            HighestLevel: request.HighestLevel,
            PreferredBeer: request.PreferredBeer
        );
    }

    public UpdateAccountCommand ToCommand(UpdateAccountRequest request, ClaimsPrincipal claims)
    {
        var (feet, inches) = ParseHeight(request.Height);
        var accountId = claims.GetAccountId();
        return new UpdateAccountCommand(
            AccountId: accountId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Birthday: request.Birthday,
            HighestLevel: request.HighestLevel,
            PreferredBeer: request.PreferredBeer,
            HeightFeet: feet,
            HeightInches: inches,
            Weight: request.Weight
        );
    }
    
    private static (int? Feet, int? Inches) ParseHeight(string? height)
    {
        if (string.IsNullOrEmpty(height))
            return (null, null);

        var data = height.Split("'");
        if (data is not [var feetStr, var inchesStr, ..])
            return (null, null);

        if (!int.TryParse(feetStr, out var feet) || !int.TryParse(inchesStr, out var inches))
            return (null, null);
        
        return (feet, inches);
    }
}
