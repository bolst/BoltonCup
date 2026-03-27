using System.Security.Claims;
using BoltonCup.Infrastructure.Extensions;

namespace BoltonCup.WebAPI.Mapping;

public interface IUserMapper
{
    CurrentUserDto? ToDto(ClaimsPrincipal claims);
}

public class UserMapper(IBriefMapper _briefMapper) : IUserMapper
{
    public CurrentUserDto? ToDto(ClaimsPrincipal claims)
    {
        var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId)
            ? null
            : new CurrentUserDto( 
                UserId: userId,
                Email: claims.FindFirstValue(ClaimTypes.Email) ?? "",
                Name: claims.FindFirstValue(ClaimTypes.Name) ?? "",
                Roles: claims.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                IsAuthenticated: claims.Identity?.IsAuthenticated ?? false,
                AccountId: claims.GetAccountIdOrDefault()
            );
    }
}