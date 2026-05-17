using BoltonCup.Core;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Mapping;
using BoltonCup.WebAPI.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Controllers;

public class TournamentRegistrationsControllerTests
{
    private const int AccountId = 42;

    private readonly Mock<ITournamentRegistrationService> _registrationService = new();
    private readonly Mock<ITournamentRegistrationMapper> _registrationMapper = new();
    private readonly TournamentRegistrationsController _controller;

    public TournamentRegistrationsControllerTests()
    {
        _controller = new TournamentRegistrationsController(
            _registrationService.Object,
            _registrationMapper.Object
        )
        {
            ControllerContext = ClaimsHelper.WithAccountId(AccountId)
        };
    }

    // ---------- GET ----------

    [Fact]
    public async Task GetMyRegistration_Exists_ReturnsOkWithDto()
    {
        var registration = new TournamentRegistration { CurrentStep = 2, Payload = "{}", IsComplete = false };
        var dto = new TournamentRegistrationDto { CurrentStep = 2, Payload = "{}", IsComplete = false };
        _registrationService
            .Setup(s => s.GetAsync(5, AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);
        _registrationMapper
            .Setup(m => m.ToDto(registration))
            .Returns(dto);

        var result = await _controller.GetMyTournamentRegistration(5);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetMyRegistration_NotFound_ReturnsNoContent()
    {
        _registrationService
            .Setup(s => s.GetAsync(5, AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TournamentRegistration?)null);
        _registrationMapper
            .Setup(m => m.ToDto(null))
            .Returns((TournamentRegistrationDto?)null);

        var result = await _controller.GetMyTournamentRegistration(5);

        result.Result.Should().BeOfType<NoContentResult>();
    }

    // ---------- POST ----------

    [Fact]
    public async Task UpdateMyRegistration_ValidRequest_CallsUpsertAndReturnsOk()
    {
        var dto = new TournamentRegistrationDto { CurrentStep = 1, IsComplete = true, Payload = "{\"data\":1}" };
        UpsertTournamentRegistrationCommand? captured = null;
        _registrationService
            .Setup(s => s.UpsertAsync(It.IsAny<UpsertTournamentRegistrationCommand>(), It.IsAny<CancellationToken>()))
            .Callback<UpsertTournamentRegistrationCommand, CancellationToken>((cmd, _) => captured = cmd)
            .Returns(Task.CompletedTask);

        var result = await _controller.UpdateMyTournamentRegistration(5, dto);

        result.Should().BeOfType<OkResult>();
        captured.Should().NotBeNull();
        captured!.TournamentId.Should().Be(5);
        captured.AccountId.Should().Be(AccountId);
        captured.CurrentStep.Should().Be(1);
        captured.IsComplete.Should().BeTrue();
        captured.Payload.Should().Be("{\"data\":1}");
    }

    [Fact]
    public async Task UpdateMyRegistration_NullPayload_PassesNullInCommand()
    {
        var dto = new TournamentRegistrationDto { CurrentStep = 0, IsComplete = false, Payload = null };
        UpsertTournamentRegistrationCommand? captured = null;
        _registrationService
            .Setup(s => s.UpsertAsync(It.IsAny<UpsertTournamentRegistrationCommand>(), It.IsAny<CancellationToken>()))
            .Callback<UpsertTournamentRegistrationCommand, CancellationToken>((cmd, _) => captured = cmd)
            .Returns(Task.CompletedTask);

        await _controller.UpdateMyTournamentRegistration(1, dto);

        captured!.Payload.Should().BeNull();
    }
}
