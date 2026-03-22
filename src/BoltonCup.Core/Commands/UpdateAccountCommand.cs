namespace BoltonCup.Core.Commands;

public sealed record UpdateAccountCommand( 
    int AccountId,
    string FirstName,
    string LastName,
    DateTime Birthday, 
    string? HighestLevel, 
    string? PreferredBeer
);