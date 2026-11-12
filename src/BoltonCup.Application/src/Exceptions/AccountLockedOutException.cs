using BoltonCup.Core.Exceptions;

namespace BoltonCup.Application.Exceptions;

public class AccountLockedOutException() : BoltonCupException("Too many attempts. Try again later or reset your password.");