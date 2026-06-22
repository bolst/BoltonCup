namespace BoltonCup.Core.Exceptions;

public sealed class TradingClosedException(int TournamentId)
    : BoltonCupException($"Tournament {TournamentId} is not open for trading.");
