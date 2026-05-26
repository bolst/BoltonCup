using System.Text;
using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using BoltonCup.Infrastructure;
using BoltonCup.Infrastructure.Settings;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Mapping;
using BoltonCup.WebAPI.Stripe;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Stripe;
using StripeEvent = Stripe.Event;

namespace BoltonCup.WebAPI.Tests.Controllers;

public class WebhooksControllerTests
{
    private const string WebhookSecret = "whsec_test";

    private readonly Mock<IStripeEventConstructor> _eventConstructor = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ITournamentPaymentService> _tournamentPaymentService = new();
    private readonly Mock<IBracketChallengeService> _bracketChallengeService = new();
    private readonly Mock<ILogger<WebhooksController>> _logger = new();
    private readonly WebhooksController _controller;

    public WebhooksControllerTests()
    {
        var settings = Options.Create(new StripeSettings { WebhookSecret = WebhookSecret });

        _controller = new WebhooksController(
            settings,
            _mapper.Object,
            _tournamentPaymentService.Object,
            _bracketChallengeService.Object,
            _logger.Object,
            _eventConstructor.Object
        );

        SetupHttpContext("{}");
    }

    private void SetupHttpContext(string body, string signature = "sig_test")
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        ctx.Request.Headers["Stripe-Signature"] = signature;
        _controller.ControllerContext = new ControllerContext { HttpContext = ctx };
    }

    private void SetupStripeEvent(StripeEvent stripeEvent)
    {
        _eventConstructor
            .Setup(e => e.ConstructEvent(It.IsAny<string>(), It.IsAny<string>(), WebhookSecret))
            .Returns(stripeEvent);
    }

    private static StripeEvent BuildPaymentSucceededEvent(Dictionary<string, string> metadata)
    {
        var paymentIntent = new PaymentIntent
        {
            Id = "pi_test_123",
            Metadata = metadata
        };
        return new StripeEvent
        {
            Type = EventTypes.PaymentIntentSucceeded,
            Data = new EventData { Object = paymentIntent }
        };
    }

    // ---------- Happy paths ----------

    [Fact]
    public async Task TournamentRegistration_PaymentSucceeded_ProcessesPayment()
    {
        var evt = BuildPaymentSucceededEvent(new()
        {
            [nameof(PurchaseType)] = PurchaseType.TournamentRegistration
        });
        SetupStripeEvent(evt);

        var expectedCommand = new ProcessTournamentPaymentIntentCommand(42, 7, "pi_test_123");
        _mapper
            .Setup(m => m.TryParseTournamentPaymentCommand(
                It.IsAny<PaymentIntent>(), out expectedCommand))
            .Returns(true);

        var result = await _controller.StripeWebhook();

        result.Should().BeOfType<OkResult>();
        _tournamentPaymentService.Verify(
            s => s.ProcessPaymentIntentAsync(expectedCommand, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BracketChallenge_PaymentSucceeded_ProcessesPayment()
    {
        var evt = BuildPaymentSucceededEvent(new()
        {
            [nameof(PurchaseType)] = PurchaseType.BracketChallengeRegistration
        });
        SetupStripeEvent(evt);

        var expectedCommand = new ProcessBracketChallengePaymentIntentCommand(1, "Jane", "j@test.com", "pi_test_123", true);
        _mapper
            .Setup(m => m.TryParseBracketChallengePaymentCommand(
                It.IsAny<PaymentIntent>(), out expectedCommand))
            .Returns(true);

        var result = await _controller.StripeWebhook();

        result.Should().BeOfType<OkResult>();
        _bracketChallengeService.Verify(
            s => s.ProcessPaymentIntentAsync(expectedCommand, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------- Routing / edge cases ----------

    [Fact]
    public async Task MissingPurchaseType_ReturnsOk_DoesNotProcess()
    {
        var evt = BuildPaymentSucceededEvent(new()); // no PurchaseType key
        SetupStripeEvent(evt);

        var result = await _controller.StripeWebhook();

        result.Should().BeOfType<OkResult>();
        _tournamentPaymentService.VerifyNoOtherCalls();
        _bracketChallengeService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UnknownPurchaseType_ReturnsOk_DoesNotProcess()
    {
        var evt = BuildPaymentSucceededEvent(new()
        {
            [nameof(PurchaseType)] = "SomethingElse"
        });
        SetupStripeEvent(evt);

        var result = await _controller.StripeWebhook();

        result.Should().BeOfType<OkResult>();
        _tournamentPaymentService.VerifyNoOtherCalls();
        _bracketChallengeService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MapperReturnsFalse_DoesNotProcessPayment()
    {
        var evt = BuildPaymentSucceededEvent(new()
        {
            [nameof(PurchaseType)] = PurchaseType.TournamentRegistration
        });
        SetupStripeEvent(evt);

        ProcessTournamentPaymentIntentCommand? nullCmd = null;
        _mapper
            .Setup(m => m.TryParseTournamentPaymentCommand(
                It.IsAny<PaymentIntent>(), out nullCmd!))
            .Returns(false);

        var result = await _controller.StripeWebhook();

        result.Should().BeOfType<OkResult>();
        _tournamentPaymentService.Verify(
            s => s.ProcessPaymentIntentAsync(It.IsAny<ProcessTournamentPaymentIntentCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task NonPaymentIntentEvent_ReturnsOk_DoesNotProcess()
    {
        var evt = new StripeEvent
        {
            Type = "charge.succeeded",
            Data = new EventData { Object = new Charge() }
        };
        SetupStripeEvent(evt);

        var result = await _controller.StripeWebhook();

        result.Should().BeOfType<OkResult>();
        _tournamentPaymentService.VerifyNoOtherCalls();
        _bracketChallengeService.VerifyNoOtherCalls();
    }

    // ---------- Error handling ----------

    [Fact]
    public async Task StripeException_ReturnsBadRequest()
    {
        _eventConstructor
            .Setup(e => e.ConstructEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new StripeException("bad signature"));

        var result = await _controller.StripeWebhook();

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task GeneralException_Returns500()
    {
        _eventConstructor
            .Setup(e => e.ConstructEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("unexpected"));

        var result = await _controller.StripeWebhook();

        var statusResult = result.Should().BeOfType<StatusCodeResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }
}
