namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing the state of a tournament registration flow.</summary>
public record TournamentRegistrationDto
{
    /// <summary>Gets or sets the current step in the registration process.</summary>
    public int CurrentStep { get; set; }
    /// <summary>Gets or sets the serialized payload for the current registration step.</summary>
    public string? Payload { get; set; }
    /// <summary>Gets or sets whether the registration has been completed.</summary>
    public bool IsComplete { get; set; }
}