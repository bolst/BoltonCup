namespace BoltonCup.Core.Exceptions;

public sealed class AccountNotInTournamentException(int AccountId, int TournamentId) 
    : BoltonCupException($"Account {AccountId} is not registered for Tournament {TournamentId}.");