using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class NewsPostServiceTests
{
    const int LabelId = 1;
    const int TaggedPublishedId = 10;
    const int UntaggedPublishedId = 11;
    const int TaggedDraftId = 12;

    static IDbContextFactory<BoltonCupDbContext> NewFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<BoltonCupDbContext>(o => o.UseInMemoryDatabase($"news-{Guid.NewGuid()}"));
        return services.BuildServiceProvider().GetRequiredService<IDbContextFactory<BoltonCupDbContext>>();
    }

    static NewsPostService NewService(IDbContextFactory<BoltonCupDbContext> factory, IStorageService? storage = null, IAssetKeyGenerator? keys = null)
        => new(factory, storage ?? Mock.Of<IStorageService>(), keys ?? Mock.Of<IAssetKeyGenerator>());

    static async Task<(IDbContextFactory<BoltonCupDbContext> Factory, BoltonCupDbContext Db)> SeedAsync()
    {
        var factory = NewFactory();
        var db = await factory.CreateDbContextAsync();
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
        db.ChangeTracker.Clear();
        return (factory, db);
    }

    [Fact]
    public async Task GetPublishedAsync_ExcludesDrafts_NewestFirst()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var result = await NewService(factory).GetPublishedAsync(new GetNewsPostsQuery());

        result.Items.Select(p => p.Id).Should().Equal(UntaggedPublishedId, TaggedPublishedId);
        result.Total.Should().Be(2);
    }

    [Theory]
    [InlineData("Game Highlights")]
    [InlineData("game highlights")]
    [InlineData("  GAME HIGHLIGHTS ")]
    public async Task GetPublishedAsync_LabelFilter_IsCaseAndWhitespaceInsensitive(string label)
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var result = await NewService(factory).GetPublishedAsync(new GetNewsPostsQuery { Label = label });

        result.Items.Select(p => p.Id).Should().Equal(TaggedPublishedId);
    }

    [Fact]
    public async Task GetPublishedAsync_BlankLabel_MeansNoFilter()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var result = await NewService(factory).GetPublishedAsync(new GetNewsPostsQuery { Label = "   " });

        result.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetPublishedAsync_LoadsLabelNavigation()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var result = await NewService(factory).GetPublishedAsync(new GetNewsPostsQuery { Label = "Game Highlights" });

        result.Items.Single().Tags.Single().Label!.Name.Should().Be("Game Highlights");
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_ReturnsPublishedPost()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var post = await NewService(factory).GetPublishedBySlugAsync("older-tagged");

        post.Should().NotBeNull();
        post!.Id.Should().Be(TaggedPublishedId);
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_IgnoresDrafts()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var post = await NewService(factory).GetPublishedBySlugAsync("draft");

        post.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCoverImageAsync_CopiesAssetAndStoresFinalKey()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;
        var storage = new Mock<IStorageService>();
        var keys = new Mock<IAssetKeyGenerator>();
        keys.Setup(k => k.GenerateFinalKey<NewsPost>(TaggedPublishedId.ToString(), "coverimage", ".png"))
            .Returns("media/newspost/10/coverimage/final.png");

        await NewService(factory, storage.Object, keys.Object).UpdateCoverImageAsync(TaggedPublishedId, "temp_uploads/abc.png");

        storage.Verify(s => s.CopyAssetAsync("temp_uploads/abc.png", "media/newspost/10/coverimage/final.png", It.IsAny<CancellationToken>()), Times.Once);
        var post = await db.NewsPosts.AsNoTracking().SingleAsync(p => p.Id == TaggedPublishedId);
        post.CoverImage.Should().Be("media/newspost/10/coverimage/final.png");
    }

    [Fact]
    public async Task ReserveSlugsAsync_KeepsFreeSlugs_AndSuffixesCollisions()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var reserved = await NewsService(factory).ReserveSlugsAsync(
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
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var reserved = await NewsService(factory).ReserveSlugsAsync(
        [
            new SlugCandidate(TaggedPublishedId, "older-tagged"),
            new SlugCandidate(0, "older-tagged"),
        ]);

        reserved.Should().Equal("older-tagged", "older-tagged-2");
    }

    [Fact]
    public async Task ReserveSlugsAsync_SkipsSuffixesAlreadyInUse()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;
        db.NewsPosts.Add(new NewsPost { Id = 20, Title = "x", Slug = "older-tagged-2" });
        await db.SaveChangesAsync();

        var reserved = await NewsService(factory).ReserveSlugsAsync([new SlugCandidate(0, "older-tagged")]);

        reserved.Should().Equal("older-tagged-3");
    }

    static NewsPostService NewsService(IDbContextFactory<BoltonCupDbContext> factory) => NewService(factory);
}
