using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Infrastructure.Services;

public class EmailSender(ILogger<EmailSender> _logger) : IEmailSender<BoltonCupUser> 
{
    public Task SendConfirmationLinkAsync(BoltonCupUser user, string email, string confirmationLink)
    {
        _logger.LogInformation("Sending confirmation link to {UserName} with email {email}", user.UserName, email);
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(BoltonCupUser user, string email, string resetLink)
    {
        _logger.LogInformation("Sending password reset link to {UserName} with email {email}", user.UserName, email);
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(BoltonCupUser user, string email, string resetCode)
    {
        _logger.LogInformation("Sending password reset code to {UserName} with email {email}", user.UserName, email);
        throw new NotImplementedException();
    }
}