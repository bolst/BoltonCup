using System.Security.Claims;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps <see cref="ClaimsPrincipal"/> to the current user DTO.</summary>
public interface IUserMapper
{
    /// <summary>Maps claims from the current principal to a <see cref="CurrentUserDto"/>, or returns <see langword="null"/> if the principal has no identity.</summary>
    CurrentUserDto? ToDto(ClaimsPrincipal claims);
}

/// <summary>Maps <see cref="ClaimsPrincipal"/> to the current user DTO.</summary>
public class UserMapper : IUserMapper
{
    /// <inheritdoc/>
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
                AccountId: claims.GetAccountIdOrDefault(),
                TeamGmIds: claims.GetTeamGmIds(),
                TournamentGmIds: claims.GetTournamentGmIds()
            );
    }
}