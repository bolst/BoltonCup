namespace BoltonCup.WebAPI.Mapping.Auth;

public class UserInfoDto
{
    public int? Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public bool IsAuthenticated { get; set; }
    public List<string> Roles { get; set; }

    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? HighestLevel { get; set; }
    public string? Avatar { get; set; }
    public string? Banner { get; set; }
    public string? PreferredBeer { get; set; }
}