using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Mapping;

public class TournamentPaymentMapperTests
{
    private readonly Mapper _mapper = new(new Mock<IAssetUrlResolver>().Object);

    // ---------- ToDto ----------

    [Fact]
    public void ToDto_ValidPaymentIntent_MapsAllFields()
    {
        var breakdown = new List<PaymentBreakdown>
        {
            new(2000m, "Registration Fee"),
            new(500m, "Processing Fee", "Stripe fee")
        };
        var paymentIntent = new TournamentPaymentIntent(
            AccountId: 42,
            TournamentId: 5,
            Amount: 2500m,
            Currency: "USD",
            Secret: "secret_abc123",
            AmountBreakdown: breakdown
        );

        var dto = _mapper.ToDto(paymentIntent);

        dto.ClientSecret.Should().Be("secret_abc123");
        dto.TotalAmount.Should().Be(2500m);
        dto.Currency.Should().Be("USD");
        dto.Breakdown.Should().HaveCount(2);
    }

    [Fact]
    public void ToDto_EmptyBreakdown_MapsCorrectly()
    {
        var paymentIntent = new TournamentPaymentIntent(
            AccountId: 1,
            TournamentId: 1,
            Amount: 1000m,
            Currency: "CAD",
            Secret: "secret_xyz",
            AmountBreakdown: []
        );

        var dto = _mapper.ToDto(paymentIntent);

        dto.Breakdown.Should().BeEmpty();
    }

    // ---------- ToCommand ----------

    [Fact]
    public void ToCommand_ValidRequest_MapsAllFields()
    {
        var request = new CreateTournamentPaymentIntentRequest { Position = "Goalie" };

        var command = _mapper.ToCommand(tournamentId: 7, accountId: 42, request);

        command.AccountId.Should().Be(42);
        command.TournamentId.Should().Be(7);
        command.Position.Should().Be("Goalie");
    }

    [Fact]
    public void ToCommand_NullPosition_MapsCorrectly()
    {
        var request = new CreateTournamentPaymentIntentRequest { Position = null };

        var command = _mapper.ToCommand(tournamentId: 1, accountId: 2, request);

        command.Position.Should().BeNull();
    }
}
