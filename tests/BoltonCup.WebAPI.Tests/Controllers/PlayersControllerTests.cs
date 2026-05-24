using BoltonCup.Core;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Controllers;

public class PlayersControllerTests
{
    private readonly Mock<IPlayerRepository> _playerRepository = new();
    private readonly Mock<IPlayerMapper> _playerMapper = new();
    private readonly PlayersController _controller;

    public PlayersControllerTests()
    {
        _controller = new PlayersController(_playerRepository.Object, _playerMapper.Object);
    }

    [Fact]
    public async Task GetPlayers_CallsRepositoryAndMapper()
    {
        var request = new GetPlayersRequest { TournamentId = 1, Page = 1, Size = 10 };
        var query = new GetPlayersQuery { TournamentId = 1, Page = 1, Size = 10 };

        _playerMapper.Setup(m => m.ToQuery(request)).Returns(query);
        _playerRepository.Setup(r => r.GetAllAsync(It.IsAny<GetPlayersQuery>())).ReturnsAsync(Mock.Of<IPagedList<Player>>());
        _playerMapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<Player>>())).Returns(Mock.Of<IPagedList<PlayerDto>>());

        var result = await _controller.GetPlayers(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        _playerRepository.Verify(r => r.GetAllAsync(It.IsAny<GetPlayersQuery>()), Times.Once);
        _playerMapper.Verify(m => m.ToDtoList(It.IsAny<IPagedList<Player>>()), Times.Once);
    }

    [Fact]
    public async Task GetPlayers_WithFilters_PassesFiltersToQuery()
    {
        var request = new GetPlayersRequest { TeamId = 5, TournamentId = 2, SortBy = "jerseyNumber" };

        _playerMapper.Setup(m => m.ToQuery(request)).Returns(new GetPlayersQuery { TeamId = 5, TournamentId = 2, SortBy = "jerseyNumber" });
        _playerRepository.Setup(r => r.GetAllAsync(It.IsAny<GetPlayersQuery>())).ReturnsAsync(Mock.Of<IPagedList<Player>>());
        _playerMapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<Player>>())).Returns(Mock.Of<IPagedList<PlayerDto>>());

        await _controller.GetPlayers(request);

        _playerRepository.Verify(r => r.GetAllAsync(It.Is<GetPlayersQuery>(q =>
            q.TeamId == 5 && q.TournamentId == 2 && q.SortBy == "jerseyNumber"
        )), Times.Once);
    }

    [Fact]
    public async Task GetPlayerById_WhenFound_ReturnsOk()
    {
        _playerRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Mock.Of<Player>(p => p.TournamentId == 1 && p.AccountId == 5));
        _playerMapper.Setup(m => m.ToDto(It.IsAny<Player>())).Returns(Mock.Of<PlayerSingleDto>());

        var result = await _controller.GetPlayerById(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPlayerById_NotFound_ReturnsNoContent()
    {
        _playerRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Player?)null);
        _playerMapper.Setup(m => m.ToDto(null)).Returns((PlayerSingleDto?)null);

        var result = await _controller.GetPlayerById(1);

        result.Result.Should().BeOfType<NoContentResult>();
    }
}
