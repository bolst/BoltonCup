using BoltonCup.Core;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.WebAPI.Mapping.Auth;

public interface IIdentityUserMapper
{
    UserDto? ToDto(IdentityUser? user);
}

public class IdentityUserMapper(IAssetUrlResolver _urlResolver) : IIdentityUserMapper
{
    public UserDto? ToDto(IdentityUser? user)
    {
        return user is null
            ? null
            : new UserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };
    }
}