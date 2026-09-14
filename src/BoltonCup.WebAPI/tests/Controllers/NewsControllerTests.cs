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

public class NewsControllerTests
{
    readonly Mock<INewsPostService> _news = new();
    readonly Mock<IMapper> _mapper = new();
    readonly NewsController _controller;

    public NewsControllerTests()
    {
        _controller = new NewsController(_news.Object, _mapper.Object)
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
    public async Task GetNewsPosts_CallsServiceAndMapper_ReturnsOk()
    {
        var request = new GetNewsPostsRequest { Page = 1, Size = 10 };
        _mapper.Setup(m => m.ToQuery(request)).Returns(new GetNewsPostsQuery { Page = 1, Size = 10 });
        _news.Setup(s => s.GetPublishedAsync(It.IsAny<GetNewsPostsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IPagedList<NewsPost>>());
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<NewsPost>>())).Returns(Mock.Of<IPagedList<NewsPostDto>>());

        var result = await _controller.GetNewsPosts(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        _news.Verify(s => s.GetPublishedAsync(It.IsAny<GetNewsPostsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapper.Verify(m => m.ToDtoList(It.IsAny<IPagedList<NewsPost>>()), Times.Once);
    }

    [Fact]
    public async Task GetNewsPosts_PassesLabelToQuery()
    {
        var request = new GetNewsPostsRequest { Label = "Game Highlights" };
        _mapper.Setup(m => m.ToQuery(request)).Returns(new GetNewsPostsQuery { Label = "Game Highlights" });
        _news.Setup(s => s.GetPublishedAsync(It.IsAny<GetNewsPostsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IPagedList<NewsPost>>());
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<NewsPost>>())).Returns(Mock.Of<IPagedList<NewsPostDto>>());

        await _controller.GetNewsPosts(request);

        _news.Verify(s => s.GetPublishedAsync(It.Is<GetNewsPostsQuery>(q => q.Label == "Game Highlights"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNewsPosts_RepeatedRequest_IsServedFromCache()
    {
        var request = new GetNewsPostsRequest { Page = 2, Size = 5, Label = "Game Highlights" };
        _mapper.Setup(m => m.ToQuery(It.IsAny<GetNewsPostsRequest>())).Returns(new GetNewsPostsQuery());
        _news.Setup(s => s.GetPublishedAsync(It.IsAny<GetNewsPostsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IPagedList<NewsPost>>());
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<NewsPost>>())).Returns(Mock.Of<IPagedList<NewsPostDto>>());

        await _controller.GetNewsPosts(request);
        await _controller.GetNewsPosts(new GetNewsPostsRequest { Page = 2, Size = 5, Label = " game highlights " });

        _news.Verify(s => s.GetPublishedAsync(It.IsAny<GetNewsPostsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNewsPostBySlug_WhenFound_ReturnsOk()
    {
        var post = new NewsPost { Id = 1, Title = "Finals recap", Slug = "finals-recap", IsPublished = true };
        _news.Setup(s => s.GetPublishedBySlugAsync("finals-recap", It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _mapper.Setup(m => m.ToDto(post)).Returns(new NewsPostSingleDto { Id = 1, Title = "Finals recap", Slug = "finals-recap", Tags = [] });

        var result = await _controller.GetNewsPostBySlug("finals-recap");

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetNewsPostBySlug_NotFound_ReturnsNoContent()
    {
        _news.Setup(s => s.GetPublishedBySlugAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((NewsPost?)null);
        _mapper.Setup(m => m.ToDto((NewsPost?)null)).Returns((NewsPostSingleDto?)null);

        var result = await _controller.GetNewsPostBySlug("missing");

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetNewsPostBySlug_RepeatedHit_IsServedFromCache()
    {
        var post = new NewsPost { Id = 1, Title = "Finals recap", Slug = "finals-recap", IsPublished = true };
        _news.Setup(s => s.GetPublishedBySlugAsync("finals-recap", It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _mapper.Setup(m => m.ToDto(post)).Returns(new NewsPostSingleDto { Id = 1, Title = "Finals recap", Slug = "finals-recap", Tags = [] });

        await _controller.GetNewsPostBySlug("finals-recap");
        await _controller.GetNewsPostBySlug("finals-recap");

        _news.Verify(s => s.GetPublishedBySlugAsync("finals-recap", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNewsPostBySlug_RepeatedMiss_IsNotCached()
    {
        _news.Setup(s => s.GetPublishedBySlugAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((NewsPost?)null);
        _mapper.Setup(m => m.ToDto((NewsPost?)null)).Returns((NewsPostSingleDto?)null);

        await _controller.GetNewsPostBySlug("missing");
        await _controller.GetNewsPostBySlug("missing");

        _news.Verify(s => s.GetPublishedBySlugAsync("missing", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetNewsPostBySlug_SlugTooLong_ReturnsNoContentWithoutCallingService()
    {
        var result = await _controller.GetNewsPostBySlug(new string('a', 101));

        result.Result.Should().BeOfType<NoContentResult>();
        _news.Verify(s => s.GetPublishedBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetNewsPostBySlug_SlugAtMaxLength_StillCallsService()
    {
        var slug = new string('a', 100);
        _news.Setup(s => s.GetPublishedBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync((NewsPost?)null);
        _mapper.Setup(m => m.ToDto((NewsPost?)null)).Returns((NewsPostSingleDto?)null);

        await _controller.GetNewsPostBySlug(slug);

        _news.Verify(s => s.GetPublishedBySlugAsync(slug, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNewsPostBySlug_LowercasesIncomingSlug()
    {
        _news.Setup(s => s.GetPublishedBySlugAsync("opening-night", It.IsAny<CancellationToken>())).ReturnsAsync((NewsPost?)null);
        _mapper.Setup(m => m.ToDto((NewsPost?)null)).Returns((NewsPostSingleDto?)null);

        await _controller.GetNewsPostBySlug(" Opening-Night ");

        _news.Verify(s => s.GetPublishedBySlugAsync("opening-night", It.IsAny<CancellationToken>()), Times.Once);
    }
}
