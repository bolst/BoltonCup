using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Services;

public class EmailSender<TUser> : IEmailSender<TUser> 
    where TUser : class
{
    public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
}