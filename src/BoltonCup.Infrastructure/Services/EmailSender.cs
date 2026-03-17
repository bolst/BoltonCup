using System.Text;
using BoltonCup.Core;
using BoltonCup.Infrastructure.EmailTemplates;
using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

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
            ConfirmationLink = confirmationLink,
            LogoUrl = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "",
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
            ResetLink = resetLink,
            LogoUrl = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "",
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
        var decodedBytes = WebEncoders.Base64UrlDecode(resetCode);
        var cleanCode = Encoding.UTF8.GetString(decodedBytes);

        var model = new PasswordResetCodeViewModel
        {
            ResetCode = cleanCode,
            LogoUrl = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "",
        };
        var payload = new EmailPayload(
            Email: email,
            Subject: $"Your Bolton Cup code is {cleanCode}",
            TemplateName: "PasswordResetCode.PasswordResetCode",
            Model: model
        );
        await _queue.EnqueueAsync(payload);
    }
}