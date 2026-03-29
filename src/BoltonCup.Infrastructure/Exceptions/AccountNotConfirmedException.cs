using BoltonCup.Core.Exceptions;

namespace BoltonCup.Infrastructure.Exceptions;

public class AccountNotConfirmedException() 
    : BoltonCupException("Account is not confirmed.");