using System.Security.Claims;

namespace BoltonCup.WebAPI.Dtos.Auth;

public class UserInfoDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public bool IsAuthenticated { get; set; }
    public List<string> Roles { get; set; }

    public UserInfoDto(ClaimsPrincipal user)
    {
        Id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        Email = user.FindFirstValue(ClaimTypes.Email) ?? "";
        Name = user.Identity?.Name ?? "";
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false;
        Roles = user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }
}