using BoltonCup.Core;
using BoltonCup.Infrastructure.EmailTemplates;
using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Services;

public class EmailSender(
    IEmailQueue _queue,
    IAssetUrlResolver _urlResolver
) : IEmailSender<BoltonCupUser>
{
    public async Task SendConfirmationLinkAsync(BoltonCupUser user, string email, string confirmationLink)
    {
        var model = new ConfirmationEmailViewModel
        {
            FirstName = user.UserName ?? "Playa",
            ConfirmationLink = confirmationLink,
            Logo = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "",
        };
        var payload = new EmailPayload(
            Email: email,
            Subject: "Confirm your email for Bolton Cup",
            TemplateName: "Confirmation.ConfirmationEmail",
            Model: model
        );
        await _queue.EnqueueAsync(payload);
    }

    public async Task SendPasswordResetLinkAsync(BoltonCupUser user, string email, string resetLink)
    {
        var model = new PasswordResetLinkViewModel
        {
            FirstName = user.UserName ?? "Playa",
            Email = email,
            ResetLink = resetLink,
            Logo = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "",
        };
        var payload = new EmailPayload(
            Email: email,
            Subject: "Reset your password for Bolton Cup",
            TemplateName: "PasswordResetLink.PasswordResetLink",
            Model: model
        );
        await _queue.EnqueueAsync(payload);
    }

    public async Task SendPasswordResetCodeAsync(BoltonCupUser user, string email, string resetCode)
    {
        var model = new PasswordResetCodeViewModel
        {
            FirstName = user.UserName ?? "Playa",
            Email = email,
            ResetCode = resetCode,
            Logo = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "",
        };
        var payload = new EmailPayload(
            Email: email,
            Subject: "Password reset code for Bolton Cup",
            TemplateName: "PasswordResetCode.PasswordResetCode",
            Model: model
        );
        await _queue.EnqueueAsync(payload);
    }
}