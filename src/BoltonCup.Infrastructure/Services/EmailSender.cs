using BoltonCup.Core;
using BoltonCup.Infrastructure.EmailTemplates;
using BoltonCup.Infrastructure.Identity;

namespace BoltonCup.Infrastructure.Services;

public interface IEmailer
{
    Task SendConfirmationCodeAsync(BoltonCupUser user, string email, string confirmationCode);
    Task SendPasswordResetCodeAsync(BoltonCupUser user, string email, string resetCode);
    Task SendBracketChallengeCredentialsAsync(Core.BracketChallenge.Event bracketChallenge, string email);
}

public class EmailSender(
    IEmailQueue _queue,
    IAssetUrlResolver _urlResolver
) : IEmailer
{
    public async Task SendConfirmationCodeAsync(BoltonCupUser user, string email, string confirmationCode)
    {
        var model = new ConfirmationCodeViewModel
        {
            ConfirmationCode = confirmationCode,
            LogoUrl = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "",
        };
        var payload = new EmailPayload(
            Email: email,
            Subject: $"{confirmationCode} is your Bolton Cup confirmation code",
            TemplateName: "ConfirmationCode.ConfirmationCode",
            Model: model
        );
        await _queue.EnqueueAsync(payload);
    }

    public async Task SendPasswordResetCodeAsync(BoltonCupUser user, string email, string resetCode)
    {
        var model = new PasswordResetCodeViewModel
        {
            ResetCode = resetCode,
            LogoUrl = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "",
        };
        var payload = new EmailPayload(
            Email: email,
            Subject: $"{resetCode} is your Bolton Cup password reset code",
            TemplateName: "PasswordResetCode.PasswordResetCode",
            Model: model
        );
        await _queue.EnqueueAsync(payload);
    }


    public async Task SendBracketChallengeCredentialsAsync(Core.BracketChallenge.Event bracketChallenge, string email)
    {
        var model = new BracketChallengeCredentialsViewModel
        {
            Title = bracketChallenge.Title ?? "",
            Link = bracketChallenge.Link ?? "",
            Password = bracketChallenge.Password ?? "", 
            LogoUrl = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? ""
        };
        var payload = new EmailPayload(
            Email: email,
            Subject: $"{bracketChallenge.Title} Credentials",
            TemplateName: "BracketChallengeCredentials.BracketChallengeCredentials",
            Model: model
        );
        await _queue.EnqueueAsync(payload);
    }
}