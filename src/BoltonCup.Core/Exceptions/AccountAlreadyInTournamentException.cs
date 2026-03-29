namespace BoltonCup.Core.Exceptions;

public sealed class AccountAlreadyInTournamentException(int AccountId, int TournamentId) 
    : BoltonCupException($"Account {AccountId} is already in Tournament {TournamentId}.");