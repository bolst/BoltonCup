using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Exceptions;
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
        
        // Auth
        new(
            ExceptionType: typeof(AccountNotConfirmedException),
            StatusCode: StatusCodes.Status403Forbidden,
            ErrorType: ErrorTypes.Auth.AccountNotConfirmed,
            Title: "Account not confirmed."
        ),
        new(
            ExceptionType: typeof(InvalidCredentialsException),
            StatusCode: StatusCodes.Status401Unauthorized,
            ErrorType: ErrorTypes.Auth.InvalidCredentials,
            Title: "Invalid credentials."
        ),
        new(
            ExceptionType: typeof(UserRegistrationFailedException),
            StatusCode: StatusCodes.Status400BadRequest,
            ErrorType: ErrorTypes.Auth.UserRegistrationFailed,
            Title: "Unable to register."
        ),
        
        // Tournaments
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
        
        // Bracket Challenges
        new(
            ExceptionType: typeof(EmailAlreadyInBracketChallengeException),
            StatusCode: StatusCodes.Status409Conflict,
            ErrorType: ErrorTypes.BracketChallenges.EmailAlreadyRegistered,
            Title: "Email already registered for this bracket challenge."
        ),
        new(
            ExceptionType: typeof(BracketChallengeRegistrationClosedException),
            StatusCode: StatusCodes.Status409Conflict,
            ErrorType: ErrorTypes.BracketChallenges.RegistrationClosed,
            Title: "Registration is closed for this bracket challenge."
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