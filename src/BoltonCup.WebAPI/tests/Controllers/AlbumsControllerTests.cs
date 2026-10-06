using System.ComponentModel.DataAnnotations;
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

public class AlbumsControllerTests
{
    readonly Mock<IAlbumService> _albums = new();
    readonly Mock<IMapper> _mapper = new();
    readonly AlbumsController _controller;

    public AlbumsControllerTests()
    {
        _controller = new AlbumsController(_albums.Object, _mapper.Object)
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
    public async Task GetAlbums_CallsServiceAndMapper_ReturnsOk()
    {
        var request = new GetAlbumsRequest { Page = 1, Size = 10 };
        _mapper.Setup(m => m.ToQuery(request)).Returns(new GetAlbumsQuery { Page = 1, Size = 10 });
        _albums.Setup(s => s.GetPublishedAsync(It.IsAny<GetAlbumsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IPagedList<Album>>());
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<Album>>())).Returns(Mock.Of<IPagedList<AlbumDto>>());

        var result = await _controller.GetAlbums(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        _albums.Verify(s => s.GetPublishedAsync(It.IsAny<GetAlbumsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapper.Verify(m => m.ToDtoList(It.IsAny<IPagedList<Album>>()), Times.Once);
    }

    [Fact]
    public async Task GetAlbums_RepeatedRequest_IsServedFromCache()
    {
        var request = new GetAlbumsRequest { Page = 2, Size = 5, Label = "Game Highlights" };
        _mapper.Setup(m => m.ToQuery(It.IsAny<GetAlbumsRequest>())).Returns(new GetAlbumsQuery());
        _albums.Setup(s => s.GetPublishedAsync(It.IsAny<GetAlbumsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IPagedList<Album>>());
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<Album>>())).Returns(Mock.Of<IPagedList<AlbumDto>>());

        await _controller.GetAlbums(request);
        await _controller.GetAlbums(new GetAlbumsRequest { Page = 2, Size = 5, Label = " game highlights " });

        _albums.Verify(s => s.GetPublishedAsync(It.IsAny<GetAlbumsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAlbumBySlug_WhenFound_ReturnsOk()
    {
        var album = new Album { Id = 1, Title = "Opening Night", Slug = "opening-night", IsPublished = true };
        _albums.Setup(s => s.GetPublishedBySlugAsync("opening-night", It.IsAny<CancellationToken>())).ReturnsAsync(album);
        _mapper.Setup(m => m.ToDto(album)).Returns(new AlbumSingleDto { Id = 1, Title = "Opening Night", Slug = "opening-night", OccurredAt = DateTime.UnixEpoch, Tags = [], Images = [] });

        var result = await _controller.GetAlbumBySlug("opening-night");

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAlbumBySlug_NotFound_ReturnsNoContent()
    {
        _albums.Setup(s => s.GetPublishedBySlugAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((Album?)null);
        _mapper.Setup(m => m.ToDto((Album?)null)).Returns((AlbumSingleDto?)null);

        var result = await _controller.GetAlbumBySlug("missing");

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetAlbumBySlug_RepeatedHit_IsServedFromCache()
    {
        var album = new Album { Id = 1, Title = "Opening Night", Slug = "opening-night", IsPublished = true };
        _albums.Setup(s => s.GetPublishedBySlugAsync("opening-night", It.IsAny<CancellationToken>())).ReturnsAsync(album);
        _mapper.Setup(m => m.ToDto(album)).Returns(new AlbumSingleDto { Id = 1, Title = "Opening Night", Slug = "opening-night", OccurredAt = DateTime.UnixEpoch, Tags = [], Images = [] });

        await _controller.GetAlbumBySlug("opening-night");
        await _controller.GetAlbumBySlug("opening-night");

        _albums.Verify(s => s.GetPublishedBySlugAsync("opening-night", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAlbumBySlug_SlugTooLong_ReturnsNoContentWithoutCallingService()
    {
        var result = await _controller.GetAlbumBySlug(new string('a', 101));

        result.Result.Should().BeOfType<NoContentResult>();
        _albums.Verify(s => s.GetPublishedBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAlbumBySlug_LowercasesIncomingSlug()
    {
        _albums.Setup(s => s.GetPublishedBySlugAsync("opening-night", It.IsAny<CancellationToken>())).ReturnsAsync((Album?)null);
        _mapper.Setup(m => m.ToDto((Album?)null)).Returns((AlbumSingleDto?)null);

        await _controller.GetAlbumBySlug(" Opening-Night ");

        _albums.Verify(s => s.GetPublishedBySlugAsync("opening-night", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAlbumImages_CallsServiceAndMapper_ReturnsOk()
    {
        var request = new GetAlbumImagesRequest { TagType = TagTargetType.Account, TagTargetId = 7, Page = 1, Size = 12 };
        _mapper.Setup(m => m.ToQuery(request)).Returns(new GetAlbumImagesQuery { TagType = TagTargetType.Account, TargetId = 7, Page = 1, Size = 12 });
        _albums.Setup(s => s.GetPublishedImagesByTagAsync(It.IsAny<GetAlbumImagesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IPagedList<AlbumImage>>());
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<AlbumImage>>())).Returns(Mock.Of<IPagedList<AlbumImageDto>>());

        var result = await _controller.GetAlbumImages(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        _albums.Verify(s => s.GetPublishedImagesByTagAsync(It.IsAny<GetAlbumImagesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAlbumImages_RepeatedRequest_IsServedFromCache()
    {
        var request = new GetAlbumImagesRequest { TagType = TagTargetType.Account, TagTargetId = 7, Page = 1, Size = 12 };
        _mapper.Setup(m => m.ToQuery(It.IsAny<GetAlbumImagesRequest>())).Returns(new GetAlbumImagesQuery());
        _albums.Setup(s => s.GetPublishedImagesByTagAsync(It.IsAny<GetAlbumImagesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IPagedList<AlbumImage>>());
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<AlbumImage>>())).Returns(Mock.Of<IPagedList<AlbumImageDto>>());

        await _controller.GetAlbumImages(request);
        await _controller.GetAlbumImages(request);

        _albums.Verify(s => s.GetPublishedImagesByTagAsync(It.IsAny<GetAlbumImagesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAlbumImages_DifferentSort_IsNotServedFromCache()
    {
        var request = new GetAlbumImagesRequest { TagType = TagTargetType.Account, TagTargetId = 7, Page = 1, Size = 12 };
        _mapper.Setup(m => m.ToQuery(It.IsAny<GetAlbumImagesRequest>())).Returns(new GetAlbumImagesQuery());
        _albums.Setup(s => s.GetPublishedImagesByTagAsync(It.IsAny<GetAlbumImagesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IPagedList<AlbumImage>>());
        _mapper.Setup(m => m.ToDtoList(It.IsAny<IPagedList<AlbumImage>>())).Returns(Mock.Of<IPagedList<AlbumImageDto>>());

        await _controller.GetAlbumImages(request);
        await _controller.GetAlbumImages(request with { SortBy = "Id", Descending = true });

        _albums.Verify(s => s.GetPublishedImagesByTagAsync(It.IsAny<GetAlbumImagesQuery>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public void GetAlbumImagesRequest_UndefinedTagType_FailsValidation()
    {
        var request = new GetAlbumImagesRequest { TagType = (TagTargetType)99, TagTargetId = 1 };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);

        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(GetAlbumImagesRequest.TagType)));
    }

    [Fact]
    public async Task StageAlbumImage_WhenStaged_ReturnsOkWithTempKey()
    {
        _albums.Setup(s => s.StagePublishedImageAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync("temp_uploads/abc.webp");

        var result = await _controller.StageAlbumImage(5);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be("temp_uploads/abc.webp");
    }

    [Fact]
    public async Task StageAlbumImage_WhenMissingOrUnpublished_ReturnsNotFound()
    {
        _albums.Setup(s => s.StagePublishedImageAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

        var result = await _controller.StageAlbumImage(5);

        result.Result.Should().BeOfType<NotFoundResult>();
    }
}