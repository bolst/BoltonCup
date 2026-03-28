using BoltonCup.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Errors;

public sealed class BoltonCupExceptionHandler(ILogger<BoltonCupExceptionHandler> _logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            EntityNotFoundException e => new ProblemDetails
            {
                Type = ErrorTypes.NotFound,
                Title = "Entity not found",
                Status = StatusCodes.Status404NotFound,
                Detail = e.Message
            },
            
            AccountAlreadyInTournamentException e => new ProblemDetails
            {
                Type = ErrorTypes.Tournaments.AccountAlreadyRegistered,
                Title = "Account already registered for tournament",
                Status = StatusCodes.Status409Conflict,
                Detail = e.Message
            },
            
            TournamentRegistrationClosedException e => new ProblemDetails
            {
                Type = ErrorTypes.Tournaments.RegistrationClosed,
                Title = "Registration closed",
                Status = StatusCodes.Status409Conflict,
                Detail = e.Message
            },
            
            BoltonCupException e => new ProblemDetails
            {
                Type = ErrorTypes.Unexpected,
                Title = "An error occurred while processing the request.",
                Status = StatusCodes.Status400BadRequest,
                Detail = e.Message
            },

            _ => null  // not ours to handle
        };

        if (problem is null)
            return false;

        _logger.LogWarning(exception, "Bolton Cup exception: {Type}", exception.GetType().Name);

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}