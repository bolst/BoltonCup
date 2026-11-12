using BoltonCup.Application.Exceptions;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Errors;

public class BoltonCupExceptionMappingsTests
{
    [Fact]
    public void TooManyAttemptsException_MapsTo429()
    {
        var problem = BoltonCupExceptionMappings.GetProblemDetails(new TooManyAttemptsException());

        problem!.Status.Should().Be(StatusCodes.Status429TooManyRequests);
        problem.Type.Should().Be(ErrorTypes.Auth.TooManyAttempts);
        problem.Detail.Should().Be("Too many attempts. Request a new code later.");
    }

    [Fact]
    public void AccountLockedOutException_MapsTo429()
    {
        var problem = BoltonCupExceptionMappings.GetProblemDetails(new AccountLockedOutException());

        problem!.Status.Should().Be(StatusCodes.Status429TooManyRequests);
        problem.Type.Should().Be(ErrorTypes.Auth.LockedOut);
        problem.Detail.Should().Be("Too many attempts. Try again later or reset your password.");
    }
}