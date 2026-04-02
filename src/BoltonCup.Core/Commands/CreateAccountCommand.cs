namespace BoltonCup.Core.Commands;

public record CreateAccountCommand( 
    string FirstName, 
    string LastName, 
    string Email,
    DateTime Birthday,
    int HeightFeet,
    int HeightInches,
    int Weight, 
    string? HighestLevel, 
    string? PreferredBeer
);