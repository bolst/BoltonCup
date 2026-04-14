namespace BoltonCup.Core.Exceptions;

public sealed class EmailAlreadyInBracketChallengeException(string Email, int TournamentId) 
    : BoltonCupException($"Email {Email} is already in Bracket Challenge {TournamentId}.");