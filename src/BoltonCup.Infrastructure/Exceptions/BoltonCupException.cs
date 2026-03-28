namespace BoltonCup.Infrastructure.Exceptions;

public class BoltonCupException(string message) 
    : Exception(message);

public class EntityNotFoundException<T>(int itemId)
    : BoltonCupException($"No {typeof(T).Name.ToLower()} with ID {itemId} found.");