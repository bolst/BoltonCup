using BoltonCup.Core.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Exceptions;

public class PasswordResetFailedException(IdentityResult identityResult)
    : BoltonCupException(identityResult.Errors.FirstOrDefault()?.Description ?? "Unable to reset password.")
{
    public IReadOnlyCollection<IdentityError> Errors { get; } = identityResult.Errors.ToArray();
}
