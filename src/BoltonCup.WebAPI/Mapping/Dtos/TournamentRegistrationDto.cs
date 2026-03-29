namespace BoltonCup.WebAPI.Mapping;

public record TournamentRegistrationDto
{
    public int CurrentStep { get; set; }
    public string? Payload { get; set; }
    public bool IsComplete { get; set; }
}