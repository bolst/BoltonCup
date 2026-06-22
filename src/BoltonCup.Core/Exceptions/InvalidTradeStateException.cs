namespace BoltonCup.Core.Exceptions;

public sealed class InvalidTradeStateException(string message)
    : BoltonCupException(message);
