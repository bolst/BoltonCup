using BoltonCup.Core.Exceptions;

namespace BoltonCup.Application.Exceptions;

public class AccountNotConfirmedException()
    : BoltonCupException("Account is not confirmed.");