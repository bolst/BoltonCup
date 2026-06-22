namespace BoltonCup.Core.Exceptions;

public sealed class PlayerNotTradeableException(Player player) 
    : BoltonCupException($"{player} cannot be traded.");