namespace BoltonCup.Core.Exceptions;

public sealed class EntityNotFoundException(string EntityType, object Id) 
    : BoltonCupException($"{EntityType} {Id} was not found.");