namespace BoltonCup.Core.Commands;

public record CreateAccountCommand( 
    string FirstName, 
    string LastName, 
    string Email,
    DateTime Birthday,
    string Height, 
    int Weight, 
    string? HighestLevel, 
    string? PreferredBeer
);