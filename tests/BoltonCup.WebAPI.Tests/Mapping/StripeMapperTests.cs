using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Stripe;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Mapping;

public class StripeMapperTests
{
    private readonly StripeMapper _mapper = new();

    private static PaymentIntent BuildPaymentIntent(string id, Dictionary<string, string> metadata) =>
        new() { Id = id, Metadata = metadata };

    // ---------------- TryParseTournamentPaymentCommand ----------------

    [Fact]
    public void TryParseTournamentPaymentCommand_ValidMetadata_ReturnsTrueWithCorrectFields()
    {
        var paymentIntent = BuildPaymentIntent("pi_tournament_123", new()
        {
            ["AccountId"] = "42",
            ["TournamentId"] = "7"
        });

        var success = _mapper.TryParseTournamentPaymentCommand(paymentIntent, out var command);

        success.Should().BeTrue();
        command.AccountId.Should().Be(42);
        command.TournamentId.Should().Be(7);
        command.PaymentId.Should().Be("pi_tournament_123");
    }

    [Fact]
    public void TryParseTournamentPaymentCommand_MissingAccountId_ReturnsFalse()
    {
        var paymentIntent = BuildPaymentIntent("pi_x", new() { ["TournamentId"] = "7" });

        var success = _mapper.TryParseTournamentPaymentCommand(paymentIntent, out _);

        success.Should().BeFalse();
    }

    [Fact]
    public void TryParseTournamentPaymentCommand_MissingTournamentId_ReturnsFalse()
    {
        var paymentIntent = BuildPaymentIntent("pi_x", new() { ["AccountId"] = "42" });

        var success = _mapper.TryParseTournamentPaymentCommand(paymentIntent, out _);

        success.Should().BeFalse();
    }

    [Theory]
    [InlineData("abc", "7")]
    [InlineData("42", "xyz")]
    [InlineData("", "7")]
    public void TryParseTournamentPaymentCommand_NonNumericId_ReturnsFalse(string accountId, string tournamentId)
    {
        var paymentIntent = BuildPaymentIntent("pi_x", new()
        {
            ["AccountId"] = accountId,
            ["TournamentId"] = tournamentId
        });

        var success = _mapper.TryParseTournamentPaymentCommand(paymentIntent, out _);

        success.Should().BeFalse();
    }

    // ---------------- TryParseBracketChallengePaymentCommand ----------------

    [Fact]
    public void TryParseBracketChallengePaymentCommand_ValidMetadata_ReturnsTrueWithCorrectFields()
    {
        var paymentIntent = BuildPaymentIntent("pi_bracket_456", new()
        {
            ["EventId"] = "12",
            ["Name"] = "Jane Doe",
            ["Email"] = "jane@example.com",
            ["AgreedToTOS"] = "true"
        });

        var success = _mapper.TryParseBracketChallengePaymentCommand(paymentIntent, out var command);

        success.Should().BeTrue();
        command.EventId.Should().Be(12);
        command.Name.Should().Be("Jane Doe");
        command.Email.Should().Be("jane@example.com");
        command.AgreedToTOS.Should().BeTrue();
        command.PaymentId.Should().Be("pi_bracket_456");
    }

    [Theory]
    [InlineData("false", false)]
    [InlineData("True", false)]   // case-sensitive: only lowercase "true" maps to true
    [InlineData("TRUE", false)]
    [InlineData("yes", false)]
    [InlineData("", false)]
    public void TryParseBracketChallengePaymentCommand_AgreedToTOS_OnlyExactLowercaseTrueIsTrue(string raw, bool expected)
    {
        var paymentIntent = BuildPaymentIntent("pi_x", new()
        {
            ["EventId"] = "1",
            ["Name"] = "n",
            ["Email"] = "e",
            ["AgreedToTOS"] = raw
        });

        var success = _mapper.TryParseBracketChallengePaymentCommand(paymentIntent, out var command);

        success.Should().BeTrue();
        command.AgreedToTOS.Should().Be(expected);
    }

    [Theory]
    [InlineData("EventId")]
    [InlineData("Name")]
    [InlineData("Email")]
    [InlineData("AgreedToTOS")]
    public void TryParseBracketChallengePaymentCommand_MissingRequiredField_ReturnsFalse(string keyToOmit)
    {
        var metadata = new Dictionary<string, string>
        {
            ["EventId"] = "1",
            ["Name"] = "n",
            ["Email"] = "e",
            ["AgreedToTOS"] = "true"
        };
        metadata.Remove(keyToOmit);
        var paymentIntent = BuildPaymentIntent("pi_x", metadata);

        var success = _mapper.TryParseBracketChallengePaymentCommand(paymentIntent, out _);

        success.Should().BeFalse();
    }

    [Fact]
    public void TryParseBracketChallengePaymentCommand_NonNumericEventId_ReturnsFalse()
    {
        var paymentIntent = BuildPaymentIntent("pi_x", new()
        {
            ["EventId"] = "not-a-number",
            ["Name"] = "n",
            ["Email"] = "e",
            ["AgreedToTOS"] = "true"
        });

        var success = _mapper.TryParseBracketChallengePaymentCommand(paymentIntent, out _);

        success.Should().BeFalse();
    }
}
