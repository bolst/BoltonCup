using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using BoltonCup.Integrations.Payments;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;
using Xunit;
using StripeEvent = Stripe.Event;

namespace BoltonCup.WebAPI.Tests.Payments;

public class StripeWebhookProcessorTests
{
    const string WebhookSecret = "whsec_test";

    readonly Mock<IStripeEventConstructor> _eventConstructor = new();
    readonly Mock<ITournamentPaymentService> _tournamentPaymentService = new();
    readonly Mock<IBracketChallengeService> _bracketChallengeService = new();
    readonly StripeWebhookProcessor _processor;

    public StripeWebhookProcessorTests()
    {
        var settings = Options.Create(new StripeSettings { WebhookSecret = WebhookSecret });
        _processor = new StripeWebhookProcessor(
            settings,
            _eventConstructor.Object,
            _tournamentPaymentService.Object,
            _bracketChallengeService.Object,
            new Mock<ILogger<StripeWebhookProcessor>>().Object
        );
    }

    void SetupStripeEvent(StripeEvent stripeEvent) =>
        _eventConstructor
            .Setup(e => e.ConstructEvent(It.IsAny<string>(), It.IsAny<string>(), WebhookSecret))
            .Returns(stripeEvent);

    static StripeEvent BuildPaymentSucceededEvent(Dictionary<string, string> metadata)
    {
        var paymentIntent = new PaymentIntent { Id = "pi_test_123", Metadata = metadata };
        return new StripeEvent
        {
            Type = EventTypes.PaymentIntentSucceeded,
            Data = new EventData { Object = paymentIntent }
        };
    }

    [Fact]
    public async Task TournamentRegistration_PaymentSucceeded_ProcessesPayment()
    {
        SetupStripeEvent(BuildPaymentSucceededEvent(new()
        {
            [nameof(PurchaseType)] = PurchaseType.TournamentRegistration,
            ["AccountId"] = "42",
            ["TournamentId"] = "7"
        }));

        await _processor.ProcessAsync("{}", "sig_test");

        _tournamentPaymentService.Verify(
            s => s.ProcessPaymentIntentAsync(
                It.Is<ProcessTournamentPaymentIntentCommand>(c => c.AccountId == 42 && c.TournamentId == 7 && c.PaymentId == "pi_test_123"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BracketChallenge_PaymentSucceeded_ProcessesPayment()
    {
        SetupStripeEvent(BuildPaymentSucceededEvent(new()
        {
            [nameof(PurchaseType)] = PurchaseType.BracketChallengeRegistration,
            ["EventId"] = "1",
            ["Name"] = "Jane",
            ["Email"] = "j@test.com",
            ["AgreedToTOS"] = "true"
        }));

        await _processor.ProcessAsync("{}", "sig_test");

        _bracketChallengeService.Verify(
            s => s.ProcessPaymentIntentAsync(
                It.Is<ProcessBracketChallengePaymentIntentCommand>(c => c.EventId == 1 && c.Name == "Jane" && c.PaymentId == "pi_test_123"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MissingPurchaseType_DoesNotProcess()
    {
        SetupStripeEvent(BuildPaymentSucceededEvent(new()));

        await _processor.ProcessAsync("{}", "sig_test");

        _tournamentPaymentService.VerifyNoOtherCalls();
        _bracketChallengeService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UnknownPurchaseType_DoesNotProcess()
    {
        SetupStripeEvent(BuildPaymentSucceededEvent(new() { [nameof(PurchaseType)] = "SomethingElse" }));

        await _processor.ProcessAsync("{}", "sig_test");

        _tournamentPaymentService.VerifyNoOtherCalls();
        _bracketChallengeService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UnparsableTournamentMetadata_DoesNotProcessPayment()
    {
        SetupStripeEvent(BuildPaymentSucceededEvent(new()
        {
            [nameof(PurchaseType)] = PurchaseType.TournamentRegistration
            // missing AccountId/TournamentId
        }));

        await _processor.ProcessAsync("{}", "sig_test");

        _tournamentPaymentService.Verify(
            s => s.ProcessPaymentIntentAsync(It.IsAny<ProcessTournamentPaymentIntentCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task NonPaymentIntentEvent_DoesNotProcess()
    {
        SetupStripeEvent(new StripeEvent
        {
            Type = "charge.succeeded",
            Data = new EventData { Object = new Charge() }
        });

        await _processor.ProcessAsync("{}", "sig_test");

        _tournamentPaymentService.VerifyNoOtherCalls();
        _bracketChallengeService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task StripeException_ThrowsStripeWebhookVerificationException()
    {
        _eventConstructor
            .Setup(e => e.ConstructEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new StripeException("bad signature"));

        var act = () => _processor.ProcessAsync("{}", "sig_test");

        await act.Should().ThrowAsync<StripeWebhookVerificationException>();
    }
}
