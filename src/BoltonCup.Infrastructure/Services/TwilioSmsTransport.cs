using BoltonCup.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BoltonCup.Infrastructure.Services;

public interface ISmsTransport
{
    Task SendAsync(string toPhoneNumber, string body, CancellationToken cancellationToken);
}

public sealed class TwilioSmsTransport : ISmsTransport
{
    readonly ITwilioRestClient _client;
    readonly TwilioSettings _settings;

    public TwilioSmsTransport(IOptions<TwilioSettings> settings, ILogger<TwilioSmsTransport> logger)
    {
        _settings = settings.Value;
        _client = new TwilioRestClient(_settings.AccountSid, _settings.AuthToken);
        logger.LogInformation("TwilioSmsTransport initialized");
    }

    public async Task SendAsync(string toPhoneNumber, string body, CancellationToken cancellationToken)
    {
        var options = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
        {
            From = new PhoneNumber(_settings.FromPhoneNumber),
            Body = body,
        };

        await MessageResource.CreateAsync(options, _client);
    }
}