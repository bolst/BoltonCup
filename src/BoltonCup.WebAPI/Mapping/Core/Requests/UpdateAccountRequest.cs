namespace BoltonCup.WebAPI.Mapping.Core;

public record UpdateAccountRequest(
    string FirstName,
    string LastName,
    DateTime Birthday,
    string? HighestLevel,
    string? PreferredBeer
);
