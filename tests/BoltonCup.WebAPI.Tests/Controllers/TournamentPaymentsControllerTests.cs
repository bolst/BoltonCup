using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Mapping;
using BoltonCup.WebAPI.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Controllers;

public class TournamentPaymentsControllerTests
{
    private const int AccountId = 42;

    private readonly Mock<ITournamentPaymentService> _paymentService = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly TournamentPaymentsController _controller;

    public TournamentPaymentsControllerTests()
    {
        _controller = new TournamentPaymentsController(
            _paymentService.Object,
            _mapper.Object
        )
        {
            ControllerContext = ClaimsHelper.WithAccountId(AccountId)
        };
    }

    private static TournamentPaymentIntent BuildPaymentIntent() =>
        new(AccountId: 42, TournamentId: 5, Amount: 1000m, Currency: "USD", Secret: "secret", AmountBreakdown: []);

    [Fact]
    public async Task CreateTournamentPaymentIntent_CallsServiceWithCorrectTournamentId()
    {
        var request = new CreateTournamentPaymentIntentRequest { Position = "Forward" };
        _mapper
            .Setup(m => m.ToCommand(It.IsAny<int>(), It.IsAny<int>(), request))
            .Returns(new CreateTournamentPaymentIntentCommand(AccountId, 5, "Forward"));
        _paymentService
            .Setup(s => s.CreateTournamentPaymentIntentAsync(It.IsAny<CreateTournamentPaymentIntentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPaymentIntent());
        _mapper
            .Setup(m => m.ToDto(It.IsAny<TournamentPaymentIntent>()))
            .Returns(new TournamentPaymentIntentDto("secret_123", 1000m, "USD", []));

        await _controller.CreateTournamentPaymentIntent(5, request);

        _mapper.Verify(m => m.ToCommand(5, AccountId, request), Times.Once);
        _paymentService.Verify(s => s.CreateTournamentPaymentIntentAsync(It.IsAny<CreateTournamentPaymentIntentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTournamentPaymentIntent_ReturnsPaymentIntentDto()
    {
        var request = new CreateTournamentPaymentIntentRequest { Position = "Goalie" };
        var expectedDto = new TournamentPaymentIntentDto("secret_abc", 2500m, "CAD", []);

        _mapper
            .Setup(m => m.ToCommand(It.IsAny<int>(), It.IsAny<int>(), request))
            .Returns(new CreateTournamentPaymentIntentCommand(AccountId, 5, "Goalie"));
        _paymentService
            .Setup(s => s.CreateTournamentPaymentIntentAsync(It.IsAny<CreateTournamentPaymentIntentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPaymentIntent());
        _mapper
            .Setup(m => m.ToDto(It.IsAny<TournamentPaymentIntent>()))
            .Returns(expectedDto);

        var result = await _controller.CreateTournamentPaymentIntent(5, request);

        result.Value.Should().Be(expectedDto);
    }
}
