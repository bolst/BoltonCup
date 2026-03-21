namespace BoltonCup.Core.Commands;

public sealed record UpdateAccountCommand
{
    public required int AccountId { get; init; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required DateTime Birthday { get; set; }
    public string? HighestLevel { get; set; }
    public string? PreferredBeer { get; set; }
}