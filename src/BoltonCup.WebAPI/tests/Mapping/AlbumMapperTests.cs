using BoltonCup.Core;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Mapping;

public class AlbumMapperTests
{
    readonly Mock<IAssetUrlResolver> _urls = new();
    readonly Mapper _mapper;

    public AlbumMapperTests()
    {
        _mapper = new Mapper(_urls.Object);
    }

    [Fact]
    public void ToDto_NullAlbum_ReturnsNull()
    {
        _mapper.ToDto((Album?)null).Should().BeNull();
    }

    [Fact]
    public void ToDto_MapsFieldsAndResolvesCoverFromCoverImage()
    {
        _urls.Setup(u => u.GetFullUrl("media/album/1/images/cover.webp")).Returns("https://cdn/cover.webp");
        var album = new Album
        {
            Id = 1,
            Title = "Opening Night",
            Slug = "opening-night",
            Description = "A great night",
            Source = "Jane Doe",
            OccurredAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            CoverImage = new AlbumImage { Id = 1, Key = "media/album/1/images/cover.webp" },
            Images = [new AlbumImage { Id = 1, Key = "media/album/1/images/cover.webp" }],
            Tags = [],
        };

        var dto = _mapper.ToDto(album)!;

        dto.Id.Should().Be(1);
        dto.Slug.Should().Be("opening-night");
        dto.Description.Should().Be("A great night");
        dto.Source.Should().Be("Jane Doe");
        dto.CoverImageUrl.Should().Be("https://cdn/cover.webp");
        dto.OccurredAt.Should().Be(album.OccurredAt);
        dto.Images.Should().ContainSingle();
    }

    [Fact]
    public void ToDto_NoCoverImage_FallsBackToFirstImage()
    {
        _urls.Setup(u => u.GetFullUrl("media/album/1/images/first.webp")).Returns("https://cdn/first.webp");
        var album = new Album
        {
            Id = 1,
            Title = "t",
            Slug = "t",
            Images =
            [
                new AlbumImage { Id = 1, Key = "media/album/1/images/first.webp" },
                new AlbumImage { Id = 2, Key = "media/album/1/images/second.webp" },
            ],
            Tags = [],
        };

        var dto = _mapper.ToDto(album)!;

        dto.CoverImageUrl.Should().Be("https://cdn/first.webp");
    }

    [Fact]
    public void ToDto_NoImages_CoverIsNull()
    {
        _urls.Setup(u => u.GetFullUrl(null)).Returns((string?)null);
        var album = new Album { Id = 1, Title = "t", Slug = "t", Images = [], Tags = [] };

        var dto = _mapper.ToDto(album)!;

        dto.CoverImageUrl.Should().BeNull();
    }

    [Fact]
    public void ToDto_MapsImageUrlsAndTags()
    {
        _urls.Setup(u => u.GetFullUrl(It.IsAny<string?>())).Returns<string?>(k => k is null ? null : $"https://cdn/{k}");
        var album = new Album
        {
            Id = 1,
            Title = "t",
            Slug = "t",
            Images =
            [
                new AlbumImage
                {
                    Id = 1,
                    Key = "a.webp",
                    Tags = [new AlbumImageTag { Id = 1, LabelId = 5, Label = new TagLabel { Id = 5, Name = "Candid" } }],
                },
            ],
            Tags = [],
        };

        var dto = _mapper.ToDto(album)!;

        var image = dto.Images.Single();
        image.Url.Should().Be("https://cdn/a.webp");
        image.Tags.Should().ContainSingle().Which.Name.Should().Be("Candid");
    }

    [Fact]
    public void ToDtoList_MapsItemsAndPreservesPaging()
    {
        _urls.Setup(u => u.GetFullUrl(It.IsAny<string?>())).Returns<string?>(k => k is null ? null : $"https://cdn/{k}");
        var paged = new Mock<IPagedList<Album>>();
        paged.SetupGet(p => p.Items).Returns(
        [
            new Album { Id = 1, Title = "A", Slug = "a", Images = [], Tags = [] },
            new Album { Id = 2, Title = "B", Slug = "b", Images = [], Tags = [] },
        ]);
        paged.SetupGet(p => p.Total).Returns(7);
        paged.SetupGet(p => p.Page).Returns(2);
        paged.SetupGet(p => p.Size).Returns(2);

        var dto = _mapper.ToDtoList(paged.Object);

        dto.Items.Select(i => i.Slug).Should().Equal("a", "b");
        (dto.Total, dto.Page, dto.Size).Should().Be((7, 2, 2));
    }

    [Fact]
    public void ToQuery_MapsAllFields()
    {
        var request = new GetAlbumsRequest { Label = "Game Highlights", Page = 2, Size = 10, SortBy = "title", Descending = true };

        var query = _mapper.ToQuery(request);

        query.Label.Should().Be("Game Highlights");
        query.Page.Should().Be(2);
        query.Size.Should().Be(10);
        query.SortBy.Should().Be("title");
        query.Descending.Should().BeTrue();
    }

    [Fact]
    public void ToQuery_MapsAlbumImagesRequest()
    {
        var request = new GetAlbumImagesRequest { TagType = TagTargetType.Account, TagTargetId = 7, Page = 2, Size = 10, SortBy = "id", Descending = true };

        var query = _mapper.ToQuery(request);

        query.TagType.Should().Be(TagTargetType.Account);
        query.TargetId.Should().Be(7);
        query.Page.Should().Be(2);
        query.Size.Should().Be(10);
        query.SortBy.Should().Be("id");
        query.Descending.Should().BeTrue();
    }

    [Fact]
    public void ToDtoList_MapsAlbumImages_PreservesPaging()
    {
        _urls.Setup(u => u.GetFullUrl(It.IsAny<string?>())).Returns<string?>(k => k is null ? null : $"https://cdn/{k}");
        var paged = new Mock<IPagedList<AlbumImage>>();
        paged.SetupGet(p => p.Items).Returns(
        [
            new AlbumImage { Id = 1, Key = "a.webp", Tags = [] },
            new AlbumImage { Id = 2, Key = "b.webp", Tags = [] },
        ]);
        paged.SetupGet(p => p.Total).Returns(5);
        paged.SetupGet(p => p.Page).Returns(1);
        paged.SetupGet(p => p.Size).Returns(2);

        var dto = _mapper.ToDtoList(paged.Object);

        dto.Items.Select(i => i.Url).Should().Equal("https://cdn/a.webp", "https://cdn/b.webp");
        (dto.Total, dto.Page, dto.Size).Should().Be((5, 1, 2));
    }
}