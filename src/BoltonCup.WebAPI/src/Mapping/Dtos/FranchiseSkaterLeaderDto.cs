namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a skater's totals across every season played for a franchise.</summary>
public record FranchiseSkaterLeaderDto
{
    /// <summary>Gets the account ID of the skater.</summary>
    public required int AccountId { get; init; }
    /// <summary>Gets the first name of the skater.</summary>
    public required string FirstName { get; init; }
    /// <summary>Gets the last name of the skater.</summary>
    public required string LastName { get; init; }
    /// <summary>Gets the URL of the skater's profile picture.</summary>
    public string? ProfilePictureUrl { get; init; }
    /// <summary>Gets the number of seasons the skater played for the franchise.</summary>
    public required int Seasons { get; init; }
    /// <summary>Gets the number of games played.</summary>
    public required int GamesPlayed { get; init; }
    /// <summary>Gets the number of goals.</summary>
    public required int Goals { get; init; }
    /// <summary>Gets the number of assists.</summary>
    public required int Assists { get; init; }
    /// <summary>Gets the number of points.</summary>
    public required int Points { get; init; }
}