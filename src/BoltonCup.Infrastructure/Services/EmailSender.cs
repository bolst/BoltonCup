using BoltonCup.Infrastructure.EmailTemplates;
using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Services;

public class EmailSender(
    IEmailQueue _queue
) : IEmailSender<BoltonCupUser>
{
    public async Task SendConfirmationLinkAsync(BoltonCupUser user, string email, string confirmationLink)
    {
        var model = new ConfirmationEmailViewModel
        {
            FirstName = user.UserName ?? "Playa",
            ConfirmationLink = confirmationLink
        };
        var payload = new EmailPayload(
            Email: email,
            Subject: "Confirm your email for Bolton Cup",
            TemplateName: "Confirmation.ConfirmationEmail",
            Model: model
        );
        await _queue.EnqueueAsync(payload);
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