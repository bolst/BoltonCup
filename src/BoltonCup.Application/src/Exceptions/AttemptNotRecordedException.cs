using BoltonCup.Core.Exceptions;

namespace BoltonCup.Application.Exceptions;

public class AttemptNotRecordedException() : BoltonCupException("Something went wrong. Please try again.");