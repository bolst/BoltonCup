namespace BoltonCup.WebAPI.Mapping;

public class AccountDto
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public DateTime Birthday { get; set; }
    public string? HighestLevel { get; set; }
    public string? Avatar { get; set; }
    public string? Banner { get; set; }
    public string? PreferredBeer { get; set; }
    public int? HeightFeet { get; set; }
    public int? HeightInches { get; set; }
    public int? Weight { get; set; }
}