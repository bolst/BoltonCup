using BoltonCup.Core;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Controllers;

public class FranchisesControllerTests
{
    readonly Mock<IFranchiseService> _franchises = new Mock<IFranchiseService>();
    readonly Mock<IMapper> _mapper = new Mock<IMapper>();
    readonly FranchisesController _controller;

    public FranchisesControllerTests()
    {
        _controller = new FranchisesController(_franchises.Object, _mapper.Object)
        {
            // The base controller's cache helper resolves IMemoryCache from the request services.
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection().AddMemoryCache().BuildServiceProvider(),
                },
            },
        };
    }

    [Fact]
    public async Task GetFranchises_CallsServiceAndMapper()
    {
        IReadOnlyList<FranchiseSummary> summaries = [];
        IReadOnlyList<FranchiseDto> dtos = [];
        _franchises.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(summaries);
        _mapper.Setup(m => m.ToDtoList(summaries)).Returns(dtos);

        var result = await _controller.GetFranchises();

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(dtos);
        _franchises.Verify(s => s.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapper.Verify(m => m.ToDtoList(summaries), Times.Once);
    }

    [Fact]
    public async Task GetFranchises_SecondCall_IsServedFromCache()
    {
        _franchises.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IReadOnlyList<FranchiseSummary>>())).Returns([]);

        await _controller.GetFranchises();
        await _controller.GetFranchises();

        _franchises.Verify(s => s.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFranchiseBySlug_ReturnsNoContent_WhenMissing()
    {
        _franchises.Setup(s => s.GetDetailBySlugAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((FranchiseDetail?)null);
        _mapper.Setup(m => m.ToDto((FranchiseDetail?)null)).Returns((FranchiseSingleDto?)null);

        var result = await _controller.GetFranchiseBySlug("missing");

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetFranchiseBySlug_Miss_IsNotCached()
    {
        _franchises.Setup(s => s.GetDetailBySlugAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((FranchiseDetail?)null);
        _mapper.Setup(m => m.ToDto((FranchiseDetail?)null)).Returns((FranchiseSingleDto?)null);

        await _controller.GetFranchiseBySlug("missing");
        await _controller.GetFranchiseBySlug("missing");

        _franchises.Verify(s => s.GetDetailBySlugAsync("missing", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetFranchiseBySlug_Hit_IsServedFromCache()
    {
        var (detail, dto) = NewDetailAndDto();
        _franchises.Setup(s => s.GetDetailBySlugAsync("franchise", It.IsAny<CancellationToken>())).ReturnsAsync(detail);
        _mapper.Setup(m => m.ToDto(detail)).Returns(dto);

        await _controller.GetFranchiseBySlug("franchise");
        var result = await _controller.GetFranchiseBySlug("franchise");

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(dto);
        _franchises.Verify(s => s.GetDetailBySlugAsync("franchise", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFranchiseBySlug_CacheIsKeyedBySlug()
    {
        var (detail, dto) = NewDetailAndDto();
        _franchises.Setup(s => s.GetDetailBySlugAsync("franchise", It.IsAny<CancellationToken>())).ReturnsAsync(detail);
        _franchises.Setup(s => s.GetDetailBySlugAsync("other", It.IsAny<CancellationToken>())).ReturnsAsync((FranchiseDetail?)null);
        _mapper.Setup(m => m.ToDto(detail)).Returns(dto);
        _mapper.Setup(m => m.ToDto((FranchiseDetail?)null)).Returns((FranchiseSingleDto?)null);

        await _controller.GetFranchiseBySlug("franchise");
        var result = await _controller.GetFranchiseBySlug("other");

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetFranchiseBySlug_NormalizesCase()
    {
        var (detail, dto) = NewDetailAndDto();
        _franchises.Setup(s => s.GetDetailBySlugAsync("franchise", It.IsAny<CancellationToken>())).ReturnsAsync(detail);
        _mapper.Setup(m => m.ToDto(detail)).Returns(dto);

        var result = await _controller.GetFranchiseBySlug(" Franchise ");

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task GetFranchiseBySlug_TooLong_ReturnsNoContent_WithoutLookup()
    {
        var result = await _controller.GetFranchiseBySlug(new string('a', 101));

        result.Result.Should().BeOfType<NoContentResult>();
        _franchises.Verify(s => s.GetDetailBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetFranchiseBySlug_ReturnsOk()
    {
        var (detail, dto) = NewDetailAndDto();
        _franchises.Setup(s => s.GetDetailBySlugAsync("franchise", It.IsAny<CancellationToken>())).ReturnsAsync(detail);
        _mapper.Setup(m => m.ToDto(detail)).Returns(dto);

        var result = await _controller.GetFranchiseBySlug("franchise");

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(dto);
    }

    static (FranchiseDetail Detail, FranchiseSingleDto Dto) NewDetailAndDto()
    {
        var detail = new FranchiseDetail
        {
            Franchise = new Franchise
            {
                Id = 5,
                Name = "Franchise",
                Slug = "franchise",
                NameShort = "F",
                Abbreviation = "FR",
                PrimaryColorHex = "#000000",
                SecondaryColorHex = "#FFFFFF",
            },
            Owners = [],
            Titles = [],
            AllTime = FranchiseRecord.Empty,
            IntraFranchiseGames = 0,
            Seasons = [],
            SkaterLeaders = [],
            GoalieLeaders = [],
        };
        var dto = new FranchiseSingleDto
        {
            Id = 5,
            Name = "Franchise",
            Slug = "franchise",
            NameShort = "F",
            Abbreviation = "FR",
            PrimaryColorHex = "#000000",
            SecondaryColorHex = "#FFFFFF",
            Owners = [],
            Titles = [],
            AllTime = new FranchiseRecordDto { GamesPlayed = 0, Wins = 0, Losses = 0, Ties = 0, GoalsFor = 0, GoalsAgainst = 0 },
            IntraFranchiseGames = 0,
            Seasons = [],
            SkaterLeaders = [],
            GoalieLeaders = [],
        };
        return (detail, dto);
    }
}