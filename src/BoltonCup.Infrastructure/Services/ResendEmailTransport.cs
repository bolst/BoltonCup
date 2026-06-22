using System.Net.Http.Headers;
using System.Net.Http.Json;
using BoltonCup.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BoltonCup.Infrastructure.Services;

public interface IEmailTransport
{
    Task SendAsync(string toEmail, string subject, string htmlBody, string textBody, CancellationToken cancellationToken);
}

public sealed class ResendEmailTransport : IEmailTransport
{
    private const string SendEndpoint = "emails";

    private readonly HttpClient _httpClient;
    private readonly ResendSettings _settings;

    public ResendEmailTransport(HttpClient httpClient, IOptions<ResendSettings> settings, ILogger<ResendEmailTransport> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        logger.LogInformation("ResendEmailTransport initialized");
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, string textBody, CancellationToken cancellationToken)
    {
        var from = string.IsNullOrWhiteSpace(_settings.SenderName)
            ? _settings.SenderEmail
            : $"{_settings.SenderName} <{_settings.SenderEmail}>";

        var payload = new
        {
            from,
            to = new[] { toEmail },
            subject,
            html = htmlBody,
            text = textBody,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, SendEndpoint)
        {
            Content = JsonContent.Create(payload),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Resend send failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
        }
    }
}
