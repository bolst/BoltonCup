using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Services;

public class EmailSender : IEmailSender<BoltonCupUser> 
{
    public Task SendConfirmationLinkAsync(BoltonCupUser user, string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(BoltonCupUser user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(BoltonCupUser user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
}