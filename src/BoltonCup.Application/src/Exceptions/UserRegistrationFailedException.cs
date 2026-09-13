using BoltonCup.Core.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Application.Exceptions;

public class UserRegistrationFailedException(IdentityResult identityResult)
    : BoltonCupException(identityResult.Errors.FirstOrDefault()?.Description ?? "Unable to create user.");