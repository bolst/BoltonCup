namespace BoltonCup.Core.Exceptions;

public sealed class TournamentRegistrationClosedException(int TournamentId) 
    : BoltonCupException($"Tournament {TournamentId} is not open for registration.");