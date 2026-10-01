using BoltonCup.Core;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Mapping;

public class NewsMapperTests
{
    readonly Mock<IAssetUrlResolver> _urls = new();
    readonly Mapper _mapper;

    public NewsMapperTests()
    {
        _urls.Setup(u => u.GetFullUrl("cover.png")).Returns("https://cdn/cover.png");
        _mapper = new Mapper(_urls.Object);
    }

    static Team NewTeam(int id, string name) => new()
    {
        Id = id,
        Name = name,
        NameShort = name,
        Abbreviation = name[..3].ToUpperInvariant(),
        PrimaryColorHex = "#000000",
        SecondaryColorHex = "#ffffff",
    };

    [Fact]
    public void ToDto_NullPost_ReturnsNull()
    {
        _mapper.ToDto((NewsPost?)null).Should().BeNull();
    }

    [Fact]
    public void ToDto_MapsFieldsResolvesCoverAndIncludesMarkdown()
    {
        var post = new NewsPost
        {
            Id = 7,
            Title = "Finals recap",
            Slug = "finals-recap",
            Summary = "Short",
            CoverImage = "cover.png",
            MarkdownContent = "# Heading",
            PublishedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            Tags = [new NewsPostTag { Id = 1, LabelId = 5, Label = new TagLabel { Id = 5, Name = "Game Highlights" } }],
        };

        var dto = _mapper.ToDto(post)!;

        dto.Id.Should().Be(7);
        dto.Slug.Should().Be("finals-recap");
        dto.CoverImageUrl.Should().Be("https://cdn/cover.png");
        dto.MarkdownContent.Should().Be("# Heading");
        dto.PublishedAt.Should().Be(post.PublishedAt);
        dto.Tags.Should().ContainSingle().Which.Name.Should().Be("Game Highlights");
    }

    [Fact]
    public void ToTagDto_ResolvesEachTargetName()
    {
        var home = NewTeam(1, "Sharks");
        var away = NewTeam(2, "Bears");
        var tags = new EntityTag[]
        {
            new NewsPostTag { Id = 1, LabelId = 5, Label = new TagLabel { Id = 5, Name = "Game Highlights" } },
            new NewsPostTag { Id = 2, TeamId = 1, Team = home },
            new NewsPostTag { Id = 3, TournamentId = 9, Tournament = new Tournament { Id = 9, Name = "Bolton Cup 2026" } },
            new NewsPostTag { Id = 4, AccountId = 3, Account = new Account { Id = 3, FirstName = "Sam", LastName = "Lee", Email = "s@x.com", Birthday = DateTime.UnixEpoch } },
            new NewsPostTag { Id = 5, GameId = 4, Game = new Game { Id = 4, TournamentId = 9, GameTime = DateTime.UnixEpoch, HomeTeam = home, AwayTeam = away } },
        };

        var dtos = tags.Select(t => _mapper.ToTagDto(t)!).ToList();

        dtos.Select(d => (d.Type, d.TargetId, d.Name)).Should().Equal(
            (TagTargetType.Label, 5, "Game Highlights"),
            (TagTargetType.Team, 1, "Sharks"),
            (TagTargetType.Tournament, 9, "Bolton Cup 2026"),
            (TagTargetType.Account, 3, "Sam Lee"),
            (TagTargetType.Game, 4, "Sharks vs Bears"));
    }

    [Fact]
    public void ToTagDto_UnloadedNavigation_FallsBackToTypeAndId()
    {
        var dto = _mapper.ToTagDto(new NewsPostTag { Id = 1, TeamId = 42 })!;

        dto.Name.Should().Be("Team 42");
    }

    [Fact]
    public void ToTagDto_NoTarget_ReturnsNull()
    {
        _mapper.ToTagDto(new NewsPostTag { Id = 1 }).Should().BeNull();
    }

    [Fact]
    public void ToDto_NullCoverImage_ResolvesThroughUrlResolver()
    {
        _urls.Setup(u => u.GetFullUrl(null)).Returns((string?)null);
        var post = new NewsPost { Id = 1, Title = "t", Slug = "t", CoverImage = null };

        var dto = _mapper.ToDto(post)!;

        dto.CoverImageUrl.Should().BeNull();
        _urls.Verify(u => u.GetFullUrl(null), Times.Once);
    }

    [Fact]
    public void ToDto_MixedTags_OrdersByTypeThenName()
    {
        var post = new NewsPost
        {
            Id = 1,
            Title = "t",
            Slug = "t",
            Tags =
            [
                new NewsPostTag { Id = 1, TeamId = 2, Team = NewTeam(2, "Wolves") },
                new NewsPostTag { Id = 2, LabelId = 5, Label = new TagLabel { Id = 5, Name = "Recap" } },
                new NewsPostTag { Id = 3, TeamId = 1, Team = NewTeam(1, "Bears") },
                new NewsPostTag { Id = 4, GameId = 9, Game = new Game { Id = 9, TournamentId = 1, GameTime = DateTime.UnixEpoch } },
            ],
        };

        var dto = _mapper.ToDto(post)!;

        dto.Tags.Select(t => (t.Type, t.Name)).Should().Equal(
            (TagTargetType.Game, "Game 9"),
            (TagTargetType.Team, "Bears"),
            (TagTargetType.Team, "Wolves"),
            (TagTargetType.Label, "Recap"));
    }

    [Fact]
    public void ToDtoList_MapsItemsAndPreservesPaging()
    {
        var paged = new Mock<IPagedList<NewsPost>>();
        paged.SetupGet(p => p.Items).Returns(
        [
            new NewsPost { Id = 1, Title = "A", Slug = "a", CoverImage = "cover.png" },
            new NewsPost { Id = 2, Title = "B", Slug = "b", MarkdownContent = "hidden" },
        ]);
        paged.SetupGet(p => p.Total).Returns(7);
        paged.SetupGet(p => p.Page).Returns(2);
        paged.SetupGet(p => p.Size).Returns(2);

        var dto = _mapper.ToDtoList(paged.Object);

        dto.Items.Select(i => i.Slug).Should().Equal("a", "b");
        dto.Items.First().CoverImageUrl.Should().Be("https://cdn/cover.png");
        dto.Items.Should().AllBeOfType<NewsPostDto>();
        (dto.Total, dto.Page, dto.Size).Should().Be((7, 2, 2));
    }
}
