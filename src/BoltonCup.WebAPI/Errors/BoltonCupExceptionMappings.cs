using BoltonCup.Core.Exceptions;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Errors;

// Let there be a defined list of exceptions with associated status codes.

public static class BoltonCupExceptionMappings
{
    public static readonly IReadOnlyList<ExceptionMappingConfig> Values =
    [
        new( 
            ExceptionType: typeof(EntityNotFoundException), 
            StatusCode: StatusCodes.Status404NotFound, 
            ErrorType: ErrorTypes.NotFound,
            Title: "Entity not found."
        ),
        new( 
            ExceptionType: typeof(AccountAlreadyInTournamentException), 
            StatusCode: StatusCodes.Status409Conflict, 
            ErrorType: ErrorTypes.Tournaments.AccountAlreadyRegistered,
            Title: "Account already registered for tournament."
        ),
        new( 
            ExceptionType: typeof(TournamentRegistrationClosedException), 
            StatusCode: StatusCodes.Status409Conflict, 
            ErrorType: ErrorTypes.Tournaments.RegistrationClosed,
            Title: "Registration closed."
        ),
        
        // Base/Fallback exception
        new( 
            ExceptionType: typeof(BoltonCupException), 
            StatusCode: StatusCodes.Status422UnprocessableEntity, 
            ErrorType: ErrorTypes.Unexpected,
            Title: "An error occurred while processing the request."
        )
    ];
    
    public static BoltonCupProblemDetails? GetProblemDetails(Exception exception)
    {
        var mapping = Values.FirstOrDefault(m => 
            m.ExceptionType.IsAssignableFrom(exception.GetType()));

        if (mapping is null)
            return null;

        return new BoltonCupProblemDetails
        {
            Type = mapping.ErrorType,
            Title = mapping.Title,
            Status = mapping.StatusCode,
            Detail = exception.Message
        };
    }

    
    public record ExceptionMappingConfig(Type ExceptionType, int StatusCode, string ErrorType, string Title);
    
}