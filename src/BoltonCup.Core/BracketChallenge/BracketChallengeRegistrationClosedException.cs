namespace BoltonCup.Core.Exceptions;

public sealed class BracketChallengeRegistrationClosedException(int EventId) 
    : BoltonCupException($"Bracket challenge {EventId} is not open for registration.");