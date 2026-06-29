namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a general manager of a team.</summary>
public record TeamGmDto
{
    /// <summary>Gets the account ID of the general manager.</summary>
    public required int AccountId { get; init; }
    /// <summary>Gets the first name of the general manager.</summary>
    public string? FirstName { get; init; }
    /// <summary>Gets the last name of the general manager.</summary>
    public string? LastName { get; init; }
    /// <summary>Gets the URL of the general manager's profile picture.</summary>
    public string? ProfilePictureUrl { get; init; }
}
