using BoltonCup.Core.Exceptions;

namespace BoltonCup.Application.Exceptions;

public class AccountLockedOutException() : BoltonCupException("Too many attempts. Try again in a few minutes.");