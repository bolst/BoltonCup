using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Mapping;
using BoltonCup.WebAPI.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Controllers;

public class DraftsControllerTests
{
    private readonly Mock<IDraftService> _draftService = new();
    private readonly Mock<IPlayerRepository> _players = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IAuthorizationService> _authService = new();
    private readonly DraftsController _controller;

    public DraftsControllerTests()
    {
        _controller = new DraftsController(_draftService.Object, _players.Object, _mapper.Object, _authService.Object);
    }

    private void SetupAuthSuccess()
    {
        _authService
            .Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object?>(),
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());
    }

    private void SetupAuthFailure()
    {
        _authService
            .Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object?>(),
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Failed());
    }

    // ---------- GetDrafts ----------

    [Fact]
    public async Task GetDrafts_CallsServiceAndMapper()
    {
        var request = new GetDraftsRequest { TournamentId = 1, Status = DraftStatus.Completed };
        var query = new GetDraftsQuery { TournamentId = 1, Status = DraftStatus.Completed };

        _mapper.Setup(m => m.ToQuery(request, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(query);
        _draftService.Setup(s => s.GetAsync(It.IsAny<GetDraftsQuery>())).ReturnsAsync(Mock.Of<IPagedList<Draft>>());
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<Draft>>())).Returns(Mock.Of<IPagedList<DraftDto>>());

        var result = await _controller.GetDrafts(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        _draftService.Verify(s => s.GetAsync(It.IsAny<GetDraftsQuery>()), Times.Once);
    }

    // ---------- GetDraftById ----------

    [Fact]
    public async Task GetDraftById_WhenFound_ReturnsOk()
    {
        SetupAuthSuccess();
        _draftService.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(new Draft());
        _mapper.Setup(m => m.ToDto(It.IsAny<Draft?>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Mock.Of<DraftSingleDto>());

        var result = await _controller.GetDraftById(5);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDraftById_NotFound_ReturnsNoContent()
    {
        SetupAuthSuccess();
        _draftService.Setup(s => s.GetByIdAsync(5)).ReturnsAsync((Draft?)null);
        _mapper.Setup(m => m.ToDto(It.IsAny<Draft?>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((DraftSingleDto?)null);

        var result = await _controller.GetDraftById(5);

        result.Result.Should().BeOfType<NoContentResult>();
    }

    // ---------- CreateDraft ----------

    [Fact]
    public async Task CreateDraft_ReturnsNewDraftId()
    {
        var request = new CreateDraftRequest { TournamentId = 1, Title = "Test Draft" };
        const int newDraftId = 42;

        _controller.ControllerContext = ClaimsHelper.WithAdminRole();
        _mapper.Setup(m => m.ToCommand(request, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(new CreateDraftCommand(1, "Test Draft", null));
        _draftService.Setup(s => s.CreateAsync(It.IsAny<CreateDraftCommand>())).ReturnsAsync(newDraftId);

        var result = await _controller.CreateDraft(request);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(newDraftId);
    }

    // ---------- DeleteDraft ----------

    [Fact]
    public async Task DeleteDraft_CallsService()
    {
        _draftService.Setup(s => s.DeleteAsync(5)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteDraft(5);

        result.Should().BeOfType<OkResult>();
        _draftService.Verify(s => s.DeleteAsync(5), Times.Once);
    }

    // ---------- GetCurrentDraftPick ----------

    [Fact]
    public async Task GetCurrentDraftPick_NotFound_ReturnsNoContent()
    {
        _draftService.Setup(s => s.GetCurrentPickAsync(5)).ReturnsAsync((DraftPick?)null);
        _mapper.Setup(m => m.ToDto(It.IsAny<DraftPick?>())).Returns((DraftPickSingleDto?)null);

        var result = await _controller.GetCurrentDraftPick(5);

        result.Result.Should().BeOfType<NoContentResult>();
    }

    // ---------- DraftPlayer ----------

    [Fact]
    public async Task DraftPlayer_NotAuthorized_ReturnsForbid()
    {
        var request = new DraftPlayerRequest { PlayerId = 1, TeamId = 2, OverallPick = 5 };
        SetupAuthFailure();

        var result = await _controller.DraftPlayer(5, request, null!);

        result.Should().BeOfType<ForbidResult>();
    }

    // ---------- GetDraftPlayerRankings ----------

    [Fact]
    public async Task GetDraftPlayerRankings_CallsServiceAndMapper()
    {
        var query = new GetDraftRankingsQuery();

        _controller.ControllerContext = ClaimsHelper.WithAdminRole();
        var rankings = new Mock<IPagedList<PlayerDraftRanking>>();
        rankings.Setup(r => r.Items).Returns([]);
        _draftService.Setup(s => s.GetDraftRankingsAsync(5, It.IsAny<GetDraftRankingsQuery>())).ReturnsAsync(rankings.Object);
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<PlayerDraftRanking>>(), It.IsAny<IReadOnlySet<int>>(), It.IsAny<TournamentAvailability>())).Returns(Mock.Of<IPagedList<DraftRankingDto>>());

        var result = await _controller.GetDraftPlayerRankings(5, query);

        result.Result.Should().BeOfType<OkObjectResult>();
        _draftService.Verify(s => s.GetDraftRankingsAsync(5, It.IsAny<GetDraftRankingsQuery>()), Times.Once);
    }
}
