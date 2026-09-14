using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class NewsPostServiceTests
{
    const int LabelId = 1;
    const int TaggedPublishedId = 10;
    const int UntaggedPublishedId = 11;
    const int TaggedDraftId = 12;

    static BoltonCupDbContext NewContext() =>
        new(new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseInMemoryDatabase($"news-{Guid.NewGuid()}")
            .Options);

    static NewsPostService NewService(BoltonCupDbContext db, IStorageService? storage = null, IAssetKeyGenerator? keys = null)
        => new(db, storage ?? Mock.Of<IStorageService>(), keys ?? Mock.Of<IAssetKeyGenerator>());

    static async Task<BoltonCupDbContext> SeedAsync()
    {
        var db = NewContext();
        db.TagLabels.Add(new TagLabel { Id = LabelId, Name = "Game Highlights" });
        db.NewsPosts.AddRange(
            new NewsPost
            {
                Id = TaggedPublishedId,
                Title = "Older tagged",
                Slug = "older-tagged",
                IsPublished = true,
                PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Tags = [new NewsPostTag { LabelId = LabelId }],
            },
            new NewsPost
            {
                Id = UntaggedPublishedId,
                Title = "Newer untagged",
                Slug = "newer-untagged",
                IsPublished = true,
                PublishedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new NewsPost
            {
                Id = TaggedDraftId,
                Title = "Draft",
                Slug = "draft",
                IsPublished = false,
                Tags = [new NewsPostTag { LabelId = LabelId }],
            });
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task GetPublishedAsync_ExcludesDrafts_NewestFirst()
    {
        await using var db = await SeedAsync();

        var result = await NewService(db).GetPublishedAsync(new GetNewsPostsQuery());

        result.Items.Select(p => p.Id).Should().Equal(UntaggedPublishedId, TaggedPublishedId);
        result.Total.Should().Be(2);
    }

    [Theory]
    [InlineData("Game Highlights")]
    [InlineData("game highlights")]
    [InlineData("  GAME HIGHLIGHTS ")]
    public async Task GetPublishedAsync_LabelFilter_IsCaseAndWhitespaceInsensitive(string label)
    {
        await using var db = await SeedAsync();

        var result = await NewService(db).GetPublishedAsync(new GetNewsPostsQuery { Label = label });

        result.Items.Select(p => p.Id).Should().Equal(TaggedPublishedId);
    }

    [Fact]
    public async Task GetPublishedAsync_BlankLabel_MeansNoFilter()
    {
        await using var db = await SeedAsync();

        var result = await NewService(db).GetPublishedAsync(new GetNewsPostsQuery { Label = "   " });

        result.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetPublishedAsync_LoadsLabelNavigation()
    {
        await using var db = await SeedAsync();

        var result = await NewService(db).GetPublishedAsync(new GetNewsPostsQuery { Label = "Game Highlights" });

        result.Items.Single().Tags.Single().Label!.Name.Should().Be("Game Highlights");
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_ReturnsPublishedPost()
    {
        await using var db = await SeedAsync();

        var post = await NewService(db).GetPublishedBySlugAsync("older-tagged");

        post.Should().NotBeNull();
        post!.Id.Should().Be(TaggedPublishedId);
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_IgnoresDrafts()
    {
        await using var db = await SeedAsync();

        var post = await NewService(db).GetPublishedBySlugAsync("draft");

        post.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCoverImageAsync_CopiesAssetAndStoresFinalKey()
    {
        await using var db = await SeedAsync();
        var storage = new Mock<IStorageService>();
        var keys = new Mock<IAssetKeyGenerator>();
        keys.Setup(k => k.GenerateFinalKey<NewsPost>(TaggedPublishedId.ToString(), "coverimage", ".png"))
            .Returns("media/newspost/10/coverimage/final.png");

        await NewService(db, storage.Object, keys.Object).UpdateCoverImageAsync(TaggedPublishedId, "temp_uploads/abc.png");

        storage.Verify(s => s.CopyAssetAsync("temp_uploads/abc.png", "media/newspost/10/coverimage/final.png", It.IsAny<CancellationToken>()), Times.Once);
        var post = await db.NewsPosts.AsNoTracking().SingleAsync(p => p.Id == TaggedPublishedId);
        post.CoverImage.Should().Be("media/newspost/10/coverimage/final.png");
    }

    [Fact]
    public async Task ReserveSlugsAsync_KeepsFreeSlugs_AndSuffixesCollisions()
    {
        await using var db = await SeedAsync();

        var reserved = await NewsService(db).ReserveSlugsAsync(
        [
            new SlugCandidate(0, "brand-new"),
            new SlugCandidate(0, "older-tagged"),
            new SlugCandidate(0, "older-tagged"),
        ]);

        reserved.Should().Equal("brand-new", "older-tagged-2", "older-tagged-3");
    }

    [Fact]
    public async Task ReserveSlugsAsync_LetsAPostKeepItsOwnSlug()
    {
        await using var db = await SeedAsync();

        var reserved = await NewsService(db).ReserveSlugsAsync(
        [
            new SlugCandidate(TaggedPublishedId, "older-tagged"),
            new SlugCandidate(0, "older-tagged"),
        ]);

        reserved.Should().Equal("older-tagged", "older-tagged-2");
    }

    [Fact]
    public async Task ReserveSlugsAsync_SkipsSuffixesAlreadyInUse()
    {
        await using var db = await SeedAsync();
        db.NewsPosts.Add(new NewsPost { Id = 20, Title = "x", Slug = "older-tagged-2" });
        await db.SaveChangesAsync();

        var reserved = await NewsService(db).ReserveSlugsAsync([new SlugCandidate(0, "older-tagged")]);

        reserved.Should().Equal("older-tagged-3");
    }

    static NewsPostService NewsService(BoltonCupDbContext db) => NewService(db);
}
