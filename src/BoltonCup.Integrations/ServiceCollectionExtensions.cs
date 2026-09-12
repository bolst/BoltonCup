using Amazon.S3;
using BoltonCup.Core;
using BoltonCup.Integrations.Email;
using BoltonCup.Integrations.Music;
using BoltonCup.Integrations.Payments;
using BoltonCup.Integrations.Sms;
using BoltonCup.Integrations.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RazorLight;

namespace BoltonCup.Integrations;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddBoltonCupIntegrations(this WebApplicationBuilder builder)
    {
        builder.AddBoltonCupEmails();
        builder.AddBoltonCupSms();
        builder.AddBoltonCupS3();
        builder.AddBoltonCupPayments();
        builder.AddBoltonCupMusic();

        return builder;
    }

    static IServiceCollection AddBoltonCupEmails(this WebApplicationBuilder builder)
    {
        var razorEngine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(EmailSender).Assembly, "BoltonCup.Integrations.Email.EmailTemplates")
            .UseMemoryCachingProvider()
            .UseOptions(new RazorLightOptions
            {
                EnableDebugMode = !builder.Environment.IsProduction(),
            })
            .Build();

        builder.Services.AddSingleton<IRazorLightEngine>(razorEngine);

        builder.Services.Configure<ResendSettings>(builder.Configuration.GetSection("Resend"));

        // Set "Resend:Enabled": false (e.g. in appsettings.Development.json) to log emails instead of sending them.
        if (builder.Configuration.GetValue("Resend:Enabled", true))
        {
            builder.Services.AddHttpClient<IEmailTransport, ResendEmailTransport>(client => client.BaseAddress = new Uri("https://api.resend.com/"));
        }
        else
        {
            builder.Services.AddSingleton<IEmailTransport, LoggingEmailTransport>();
        }

        return builder.Services
            .AddSingleton<IEmailQueue, EmailQueue>()
            .AddHostedService<EmailBackgroundService>()
            .AddTransient<IEmailer, EmailSender>();
    }

    static IServiceCollection AddBoltonCupSms(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));

        // Set "Twilio:Enabled": false (e.g. in appsettings.Development.json) to log texts instead of sending them.
        if (builder.Configuration.GetValue("Twilio:Enabled", true))
        {
            builder.Services.AddSingleton<ISmsTransport, TwilioSmsTransport>();
        }
        else
        {
            builder.Services.AddSingleton<ISmsTransport, LoggingSmsTransport>();
        }

        return builder.Services
            .AddSingleton<ISmsQueue, SmsQueue>()
            .AddHostedService<SmsBackgroundService>()
            .AddTransient<ISmsSender, SmsSender>();
    }

    static IServiceCollection AddBoltonCupS3(this WebApplicationBuilder builder)
    {
        var r2Config = builder.Configuration.GetRequiredSection("CloudflareR2");
        var accountId = r2Config["AccountId"];
        var accessKey = r2Config["AccessKey"];
        var secretKey = r2Config["SecretKey"];

        var s3Credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        var s3Config = new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            AuthenticationRegion = "auto"
        };
        return builder.Services
            .AddSingleton<IAmazonS3>(_ => new AmazonS3Client(s3Credentials, s3Config))
            .AddSingleton<IAssetKeyGenerator, AssetKeyGenerator>()
            .Replace(ServiceDescriptor.Singleton<IStorageService, ServerStorageService>());
    }

    static IServiceCollection AddBoltonCupPayments(this WebApplicationBuilder builder)
    {
        Stripe.StripeConfiguration.ApiKey = builder.Configuration.GetRequiredSection("Stripe").GetValue<string>(nameof(StripeSettings.ApiKey));
        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
        return builder.Services.AddTransient<ITournamentPaymentService, TournamentPaymentService>();
    }

    static IServiceCollection AddBoltonCupMusic(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SpotifySettings>(builder.Configuration.GetSection("Spotify"));
        builder.Services.AddHttpClient<IMusicSearchService, SpotifyMusicSearchService>();
        return builder.Services;
    }
}
