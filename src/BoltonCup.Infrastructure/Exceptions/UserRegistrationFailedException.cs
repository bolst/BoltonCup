using BoltonCup.Core.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Exceptions;

public class UserRegistrationFailedException(IdentityResult identityResult) 
    : BoltonCupException(identityResult.Errors.FirstOrDefault()?.Description ?? "Unable to create user.");