namespace BoltonCup.WebAPI.Mapping;

public record UpdateAccountRequest(
    string FirstName,
    string LastName,
    DateTime Birthday,
    string? HighestLevel,
    string? PreferredBeer
);
