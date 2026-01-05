namespace BoltonCup.WebAPI.Dtos.Summaries;

public record PlayerSummary
{
    public required int Id { get; set; }
    public required int? AccountId { get; set; }
    public required string? Position { get; set; }
    public required int? JerseyNumber { get; set; }
    public required string? FirstName { get; set; }
    public required string? LastName { get; set; }
    public required DateTime? Birthday { get; set; }
    public required string? ProfilePicture { get; set; }
}