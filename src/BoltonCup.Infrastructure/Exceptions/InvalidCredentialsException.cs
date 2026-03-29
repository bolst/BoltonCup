using BoltonCup.Core.Exceptions;

namespace BoltonCup.Infrastructure.Exceptions;

public class InvalidCredentialsException() 
    : BoltonCupException("Invalid credentials.");