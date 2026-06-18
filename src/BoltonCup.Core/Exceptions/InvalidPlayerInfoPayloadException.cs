namespace BoltonCup.Core.Exceptions;

public sealed class InvalidPlayerInfoPayloadException(string message) : BoltonCupException(message);
