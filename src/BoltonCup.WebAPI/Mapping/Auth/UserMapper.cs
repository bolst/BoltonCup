using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.WebAPI.Mapping.Core;

namespace BoltonCup.WebAPI.Mapping.Auth;

public interface IUserMapper
{
    UserInfoDto? ToDto(BoltonCupUser? user, Account? account, ClaimsPrincipal claims);
}

public class UserMapper(IAssetUrlResolver _urlResolver, IBriefMapper _briefMapper) : IUserMapper
{
    public UserInfoDto? ToDto(BoltonCupUser? user, Account? account, ClaimsPrincipal claims)
    {
        if (user is null)
            return null;
        return new UserInfoDto
        {
            Id = account?.Id,
            Email = user.Email,
            FirstName = account?.FirstName,
            LastName = account?.LastName,
            Name = (account?.FirstName +  " " + account?.LastName).Trim(),
            IsAuthenticated = claims.Identity?.IsAuthenticated ?? false,
            Roles = claims.FindAll(ClaimTypes.Role) .Select(c => c.Value) .ToList(),
            Phone = account?.Phone,
            Birthday = account?.Birthday,
            HighestLevel = account?.HighestLevel,
            Avatar = account?.Avatar,
            Banner = account?.Banner,
            PreferredBeer = account?.PreferredBeer,
        };
    }
}
