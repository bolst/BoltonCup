namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a user account.</summary>
public class AccountDto
{
    /// <summary>Gets or sets the account ID.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the account email address.</summary>
    public string? Email { get; set; }
    /// <summary>Gets or sets the account holder's first name.</summary>
    public string? FirstName { get; set; }
    /// <summary>Gets or sets the account holder's last name.</summary>
    public string? LastName { get; set; }
    /// <summary>Gets or sets the account holder's full name.</summary>
    public string? Name { get; set; }
    /// <summary>Gets or sets the account holder's phone number.</summary>
    public string? Phone { get; set; }
    /// <summary>Gets or sets the account holder's date of birth.</summary>
    public DateTime Birthday { get; set; }
    /// <summary>Gets or sets the highest level of hockey the account holder has played.</summary>
    public string? HighestLevel { get; set; }
    /// <summary>Gets or sets the URL of the account holder's avatar image.</summary>
    public string? Avatar { get; set; }
    /// <summary>Gets or sets the URL of the account holder's banner image.</summary>
    public string? Banner { get; set; }
    /// <summary>Gets or sets the account holder's preferred beer.</summary>
    public string? PreferredBeer { get; set; }
    /// <summary>Gets or sets the account holder's height in feet.</summary>
    public int? HeightFeet { get; set; }
    /// <summary>Gets or sets the account holder's height in inches.</summary>
    public int? HeightInches { get; set; }
    /// <summary>Gets or sets the account holder's weight.</summary>
    public int? Weight { get; set; }
}