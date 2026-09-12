namespace BoltonCup.Core.Exceptions;

public abstract class BoltonCupException : Exception
{
    protected BoltonCupException(string message) : base(message) { }
    protected BoltonCupException(string message, Exception inner) : base(message, inner) { }
}