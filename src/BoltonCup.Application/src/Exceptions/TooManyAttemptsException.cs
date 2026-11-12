using BoltonCup.Core.Exceptions;

namespace BoltonCup.Application.Exceptions;

public class TooManyAttemptsException() : BoltonCupException("Too many attempts. Request a new code later.");