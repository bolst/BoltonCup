namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO for a referee officiating a game.</summary>
public record RefereeDto
{
    /// <summary>Gets the referee's ID.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the referee's first name.</summary>
    public required string FirstName { get; init; }
    /// <summary>Gets the referee's last name.</summary>
    public required string LastName { get; init; }
    /// <summary>Gets the referee's full name.</summary>
    public string FullName => $"{FirstName} {LastName}";
}
