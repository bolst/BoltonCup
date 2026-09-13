using System.Text;
using BoltonCup.Integrations.Payments;
using BoltonCup.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Controllers;

public class WebhooksControllerTests
{
    readonly Mock<IStripeWebhookProcessor> _webhookProcessor = new();
    readonly Mock<ILogger<WebhooksController>> _logger = new();
    readonly WebhooksController _controller;

    public WebhooksControllerTests()
    {
        _controller = new WebhooksController(_webhookProcessor.Object, _logger.Object);
        SetupHttpContext("{}");
    }

    void SetupHttpContext(string body, string signature = "sig_test")
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        ctx.Request.Headers["Stripe-Signature"] = signature;
        _controller.ControllerContext = new ControllerContext { HttpContext = ctx };
    }

    [Fact]
    public async Task ValidWebhook_DelegatesToProcessorAndReturnsOk()
    {
        var result = await _controller.StripeWebhook();

        result.Should().BeOfType<OkResult>();
        _webhookProcessor.Verify(
            p => p.ProcessAsync(It.IsAny<string>(), "sig_test", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task VerificationException_ReturnsBadRequest()
    {
        _webhookProcessor
            .Setup(p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeWebhookVerificationException("bad signature", new Exception()));

        var result = await _controller.StripeWebhook();

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task GeneralException_Returns500()
    {
        _webhookProcessor
            .Setup(p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("unexpected"));

        var result = await _controller.StripeWebhook();

        var statusResult = result.Should().BeOfType<StatusCodeResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }
}
