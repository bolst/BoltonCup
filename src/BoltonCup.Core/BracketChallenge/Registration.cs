namespace BoltonCup.Core.BracketChallenge;

public class Registration : EntityBase
{
    public Guid Id { get; set; }
    public required Guid BracketChallengeId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PaymentId { get; set; }

    public Event BracketChallenge { get; set; } = null!;
}