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

    Task SendTradeCreatedAsync(IEnumerable<string> recipients, TradeEmailInfo info);
    Task SendTradeAcceptedAsync(IEnumerable<string> recipients, TradeEmailInfo info);
    Task SendTradeDeclinedAsync(IEnumerable<string> recipients, TradeEmailInfo info);
    Task SendTradeCancelledAsync(IEnumerable<string> recipients, TradeEmailInfo info);
    Task SendTradeApprovedAsync(IEnumerable<string> recipients, TradeEmailInfo info);
}

public sealed record BroadcastRecipient(string Email, string FirstName, string LastName);

/// <summary>Snapshot of a trade's teams and player movements used to render trade notification emails.</summary>
public sealed record TradeEmailInfo(
    string ProposingTeamName,
    string ReceivingTeamName,
    IReadOnlyList<string> PlayersFromProposing,
    IReadOnlyList<string> PlayersFromReceiving);

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

    public Task SendTradeCreatedAsync(IEnumerable<string> recipients, TradeEmailInfo info) =>
        SendTradeNotificationAsync(
            recipients,
            subject: $"Trade proposed: {info.ProposingTeamName.ToUpper()} and {info.ReceivingTeamName.ToUpper()}",
            heading: "A trade has been proposed",
            intro: $"{info.ProposingTeamName} has requested a trade with {info.ReceivingTeamName}.",
            info);

    public Task SendTradeAcceptedAsync(IEnumerable<string> recipients, TradeEmailInfo info) =>
        SendTradeNotificationAsync(
            recipients,
            subject: $"Trade accepted: {info.ProposingTeamName.ToUpper()} and {info.ReceivingTeamName.ToUpper()}",
            heading: "A trade has been accepted",
            intro: $"{info.ReceivingTeamName} has accepted {info.ProposingTeamName}'s proposed trade. It now awaits admin approval.",
            info);

    public Task SendTradeDeclinedAsync(IEnumerable<string> recipients, TradeEmailInfo info) =>
        SendTradeNotificationAsync(
            recipients,
            subject: $"Trade declined: {info.ProposingTeamName.ToUpper()} and {info.ReceivingTeamName.ToUpper()}",
            heading: "A trade has been declined",
            intro: $"{info.ReceivingTeamName} has declined {info.ProposingTeamName}'s proposed trade.",
            info);

    public Task SendTradeCancelledAsync(IEnumerable<string> recipients, TradeEmailInfo info) =>
        SendTradeNotificationAsync(
            recipients,
            subject: $"Trade cancelled: {info.ProposingTeamName.ToUpper()} and {info.ReceivingTeamName.ToUpper()}",
            heading: "A trade has been cancelled",
            intro: $"The proposed trade between {info.ProposingTeamName} and {info.ReceivingTeamName} has been cancelled.",
            info);

    public Task SendTradeApprovedAsync(IEnumerable<string> recipients, TradeEmailInfo info) =>
        SendTradeNotificationAsync(
            recipients,
            subject: $"Trade processed: {info.ProposingTeamName.ToUpper()} and {info.ReceivingTeamName.ToUpper()}",
            heading: "A trade has been processed",
            intro: $"The trade between {info.ProposingTeamName} and {info.ReceivingTeamName} has been approved and processed. Rosters have been updated.",
            info);

    private async Task SendTradeNotificationAsync(IEnumerable<string> recipients, string subject, string heading, string intro, TradeEmailInfo info)
    {
        var logoUrl = _urlResolver.GetFullUrl(AssetUrlResolver.StaticKeys.Logo) ?? "";

        foreach (var email in recipients.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var model = new TradeNotificationViewModel
            {
                Heading = heading,
                Intro = intro,
                ProposingTeamName = info.ProposingTeamName,
                ReceivingTeamName = info.ReceivingTeamName,
                PlayersFromProposing = info.PlayersFromProposing,
                PlayersFromReceiving = info.PlayersFromReceiving,
                LogoUrl = logoUrl,
            };
            var payload = new EmailPayload(
                Email: email,
                Subject: subject,
                TemplateName: "TradeNotification.TradeNotification",
                Model: model
            );
            await _queue.EnqueueAsync(payload);
        }
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