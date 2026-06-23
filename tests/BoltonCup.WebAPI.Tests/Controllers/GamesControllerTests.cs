using BoltonCup.Core;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Controllers;

public class GamesControllerTests
{
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<ISkaterStatRepository> _skaterStats = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IGameWriteService> _gameWrites = new();
    private readonly Mock<IMusicLibraryService> _music = new();
    private readonly GamesController _controller;

    public GamesControllerTests()
    {
        _controller = new GamesController(
            _games.Object,
            _skaterStats.Object,
            _mapper.Object,
            _gameWrites.Object,
            _music.Object);
    }

    [Fact]
    public async Task GetGamePlaylist_ReturnsMappedDto()
    {
        var serviceResult = new GamePlaylistResult([], []);
        var dto = new GamePlaylistDto
        {
            Tracks = [new PlaylistTrackDto { FileKey = "k1", Title = "Song" }],
            Missing = [new MissingSongRequestDto { PlayerName = "Bob", SongName = "Other" }],
        };

        _music.Setup(m => m.GetGamePlaylistAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(serviceResult);
        _mapper.Setup(m => m.ToDto(serviceResult)).Returns(dto);

        var result = await _controller.GetGamePlaylist(42);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        _music.Verify(m => m.GetGamePlaylistAsync(42, It.IsAny<CancellationToken>()), Times.Once);
    }
}
