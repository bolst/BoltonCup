namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing an owner of a franchise.</summary>
public record FranchiseOwnerDto
{
    /// <summary>Gets the account ID of the owner.</summary>
    public required int AccountId { get; init; }
    /// <summary>Gets the first name of the owner.</summary>
    public string? FirstName { get; init; }
    /// <summary>Gets the last name of the owner.</summary>
    public string? LastName { get; init; }
    /// <summary>Gets the URL of the owner's profile picture.</summary>
    public string? ProfilePictureUrl { get; init; }
}