using BoltonCup.Core;
using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.WebAPI.Mapping.Auth;

public interface IBoltonCupUserMapper
{
    UserDto? ToDto(BoltonCupUser? user);
}

public class BoltonCupUserMapper(IAssetUrlResolver _urlResolver) : IBoltonCupUserMapper
{
    public UserDto? ToDto(BoltonCupUser? user)
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