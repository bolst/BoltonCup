using BoltonCup.Core.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Exceptions;

public class UserRegistrationFailedException(IdentityResult identityResult) 
    : BoltonCupException("Account is not confirmed.");