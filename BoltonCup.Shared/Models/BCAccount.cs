using System.Security.Claims;

namespace BoltonCup.Shared.Data;

public class BCAccount
{
    public required int id { get; set; }
    public required string Roles { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public DateTime? Birthday { get; set; }
    public required string Position { get; set; }
    public required string HighestLevel { get; set; }
    public required string ProfilePicture { get; set; }
    public bool Payed { get; set; }
    public required bool IsActive { get; set; }
    public int? preferred_number1 { get; set; }
    public int? preferred_number2 { get; set; }
    public int? preferred_number3 { get; set; }
    public string? preferred_beer { get; set; }
    public Guid PCKey { get; set; }
    public bool Editable { get; set; }
    public string? Message { get; set; }
    public string? SongRequest { get; set; }
    public string? SongRequestId { get; set; }
    public int? preferred_teammate1 { get; set; }
    public int? preferred_teammate2 { get; set; }


    public string FullName => $"{FirstName} {LastName}";
    public bool IsForward => Position == "forward";
    public bool IsDefense => Position == "defense";
    public bool IsGoalie => Position == "goalie";

    public bool IsAdmin => ToClaimsPrincipal().HasClaim(x => x.Value == "admin");

    public ClaimsPrincipal ToClaimsPrincipal()
    {
        IEnumerable<Claim> claims =
        [
            new(ClaimTypes.Name, FullName),
            new(ClaimTypes.Email, Email)
        ];

        foreach (var role in Roles.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            claims = claims.Append(new Claim(ClaimTypes.Role, role));
        }

        return new(new ClaimsIdentity(claims, "BoltonCup"));
    }
}