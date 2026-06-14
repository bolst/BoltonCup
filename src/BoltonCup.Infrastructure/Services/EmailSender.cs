using BoltonCup.Core;
using BoltonCup.Infrastructure.EmailTemplates;
using BoltonCup.Infrastructure.Identity;
using Markdig;

namespace BoltonCup.Infrastructure.Services;

public interface IEmailer
{
    Task SendConfirmationCodeAsync(BoltonCupUser user, string email, string confirmationCode);
    Task SendPasswordResetCodeAsync(BoltonCupUser user, string email, string resetCode);
    Task SendBracketChallengeCredentialsAsync(Core.BracketChallenge.Event bracketChallenge, string email);

    /// <summary>
    /// Sends a custom Markdown email to many recipients. The Markdown is converted to HTML once and
    /// enqueued individually so each recipient receives a separate message (no shared To/CC).
    /// Returns the broadcast id stamped on every recipient's <see cref="EmailLog"/> for correlation.
    /// </summary>
    Task<Guid> SendBroadcastAsync(IEnumerable<BroadcastRecipient> recipients, string subject, string markdownBody, bool useLayout);
}

public sealed record BroadcastRecipient(string Email, string FirstName, string LastName);

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


    public async Task<Guid> SendBroadcastAsync(IEnumerable<BroadcastRecipient> recipients, string subject, string markdownBody, bool useLayout)
    {
        var broadcastId = Guid.NewGuid();
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var bodyHtml = Markdown.ToHtml(markdownBody ?? string.Empty, pipeline);
        var logoUrl = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "";

        foreach (var recipient in recipients)
        {
            var personalizedHtml = bodyHtml
                .Replace("{{FirstName}}", System.Net.WebUtility.HtmlEncode(recipient.FirstName))
                .Replace("{{LastName}}", System.Net.WebUtility.HtmlEncode(recipient.LastName));

            var model = new BroadcastEmailViewModel
            {
                BodyHtml = personalizedHtml,
                UseLayout = useLayout,
                LogoUrl = logoUrl,
            };
            var payload = new EmailPayload(
                Email: recipient.Email,
                Subject: subject,
                TemplateName: "Broadcast.Broadcast",
                Model: model,
                BroadcastId: broadcastId
            );
            await _queue.EnqueueAsync(payload);
        }

        return broadcastId;
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