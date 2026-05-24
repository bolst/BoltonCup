using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps account domain models to DTOs and commands.</summary>
public interface IAccountMapper
{
    /// <summary>Maps an <see cref="Account"/> to an <see cref="AccountDto"/>.</summary>
    AccountDto? ToDto(Account? account, ClaimsPrincipal claims);
    /// <summary>Maps an <see cref="Account"/> to a list of <see cref="AccountTournamentDto"/>.</summary>
    ICollection<AccountTournamentDto> ToAccountTournamentDtoList(Account? account);
    /// <summary>Maps a <see cref="CompleteUserAccountRequest"/> to a <see cref="CreateAccountCommand"/>.</summary>
    CreateAccountCommand ToCommand(CompleteUserAccountRequest request, ClaimsPrincipal claims);
    /// <summary>Maps an <see cref="UpdateAccountRequest"/> to an <see cref="UpdateAccountCommand"/>.</summary>
    UpdateAccountCommand ToCommand(UpdateAccountRequest request, ClaimsPrincipal claims);
}

/// <summary>Maps account domain models to DTOs and commands.</summary>
public class AccountMapper(IBriefMapper _briefMapper) : IAccountMapper
{
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
