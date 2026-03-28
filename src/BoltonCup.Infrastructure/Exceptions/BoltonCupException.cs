
namespace BoltonCup.Infrastructure.Exceptions;
    
public class BoltonCupException : Exception
{
    public int StatusCode { get; }

    public BoltonCupException(string message, int statusCode = 400) 
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public class EntityNotFoundException<T>(int itemId)
    : BoltonCupException($"No {typeof(T).Name.ToLower()} with ID {itemId} found.", 404);