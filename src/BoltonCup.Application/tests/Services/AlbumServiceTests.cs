using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class AlbumServiceTests
{
    const int LabelId = 1;
    const int TaggedPublishedId = 10;
    const int UntaggedPublishedId = 11;
    const int TaggedDraftId = 12;

    static IDbContextFactory<BoltonCupDbContext> NewFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<BoltonCupDbContext>(o => o.UseInMemoryDatabase($"albums-{Guid.NewGuid()}"));
        return services.BuildServiceProvider().GetRequiredService<IDbContextFactory<BoltonCupDbContext>>();
    }

    static AlbumService NewService(IDbContextFactory<BoltonCupDbContext> factory, IStorageService? storage = null, IAssetKeyGenerator? keys = null)
        => new(factory, storage ?? Mock.Of<IStorageService>(), keys ?? Mock.Of<IAssetKeyGenerator>());

    static async Task<(IDbContextFactory<BoltonCupDbContext> Factory, BoltonCupDbContext Db)> SeedAsync()
    {
        var factory = NewFactory();
        var db = await factory.CreateDbContextAsync();
        db.TagLabels.Add(new TagLabel { Id = LabelId, Name = "Game Highlights" });
        db.Albums.AddRange(
            new Album
            {
                Id = TaggedPublishedId,
                Title = "Older tagged",
                Slug = "older-tagged",
                IsPublished = true,
                OccurredAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Tags = [new AlbumTag { LabelId = LabelId }],
                Images =
                [
                    new AlbumImage { Id = 100, Key = "media/album/10/images/second.webp", SortOrder = 2 },
                    new AlbumImage { Id = 101, Key = "media/album/10/images/first.webp", SortOrder = 1 },
                ],
            },
            new Album
            {
                Id = UntaggedPublishedId,
                Title = "Newer untagged",
                Slug = "newer-untagged",
                IsPublished = true,
                OccurredAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                PublishedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new Album
            {
                Id = TaggedDraftId,
                Title = "Draft",
                Slug = "draft",
                IsPublished = false,
                Tags = [new AlbumTag { LabelId = LabelId }],
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

        var result = await NewService(factory).GetPublishedAsync(new GetAlbumsQuery());

        result.Items.Select(a => a.Id).Should().Equal(UntaggedPublishedId, TaggedPublishedId);
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

        var result = await NewService(factory).GetPublishedAsync(new GetAlbumsQuery { Label = label });

        result.Items.Select(a => a.Id).Should().Equal(TaggedPublishedId);
    }

    [Fact]
    public async Task GetPublishedAsync_CoverFallsBackToFirstImageBySortOrder()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var result = await NewService(factory).GetPublishedAsync(new GetAlbumsQuery());

        var album = result.Items.Single(a => a.Id == TaggedPublishedId);
        album.CoverImage.Should().BeNull();
        album.Images.Single().Id.Should().Be(101);
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_ReturnsImagesSortedBySortOrder()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var album = await NewService(factory).GetPublishedBySlugAsync("older-tagged");

        album.Should().NotBeNull();
        album!.Images.Select(i => i.Id).Should().Equal(101, 100);
    }

    [Fact]
    public async Task GetPublishedAsync_LoadsTagLabels()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var result = await NewService(factory).GetPublishedAsync(new GetAlbumsQuery());

        var album = result.Items.Single(a => a.Id == TaggedPublishedId);
        album.Tags.Should().ContainSingle().Which.Label!.Name.Should().Be("Game Highlights");
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_LoadsChosenCoverImage()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;
        var service = NewService(factory);
        await service.SetCoverImageAsync(TaggedPublishedId, 100);

        var album = await service.GetPublishedBySlugAsync("older-tagged");

        album!.CoverImage.Should().NotBeNull();
        album.CoverImage!.Id.Should().Be(100);
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_IgnoresDrafts()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var album = await NewService(factory).GetPublishedBySlugAsync("draft");

        album.Should().BeNull();
    }

    [Fact]
    public async Task AddImageAsync_StoresKeyAndIncrementsSortOrder()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;
        using var content = new MemoryStream([1, 2, 3]);
        var storage = new Mock<IStorageService>();
        var keys = new Mock<IAssetKeyGenerator>();
        keys.Setup(k => k.GenerateFinalKey<Album>(TaggedPublishedId.ToString(), "images", ".webp"))
            .Returns("media/album/10/images/new.webp");

        var image = await NewService(factory, storage.Object, keys.Object)
            .AddImageAsync(TaggedPublishedId, content, ".webp", "image/webp");

        image.Key.Should().Be("media/album/10/images/new.webp");
        image.SortOrder.Should().Be(3);
        storage.Verify(s => s.PutAssetAsync(content, "media/album/10/images/new.webp", "image/webp", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetCoverImageAsync_RejectsImageFromAnotherAlbum()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var act = () => NewService(factory).SetCoverImageAsync(UntaggedPublishedId, 100);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetCoverImageAsync_AcceptsImageFromSameAlbum()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        await NewService(factory).SetCoverImageAsync(TaggedPublishedId, 100);

        var album = await db.Albums.AsNoTracking().SingleAsync(a => a.Id == TaggedPublishedId);
        album.CoverImageId.Should().Be(100);
    }

    [Fact]
    public async Task ReorderImagesAsync_PersistsNewOrder()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        await NewService(factory).ReorderImagesAsync(TaggedPublishedId, [100, 101]);

        var images = await db.AlbumImages.AsNoTracking().Where(i => i.AlbumId == TaggedPublishedId).OrderBy(i => i.SortOrder).ToListAsync();
        images.Select(i => i.Id).Should().Equal(100, 101);
    }

    [Fact]
    public async Task ApplyAlbumTagsToImagesAsync_CopiesAlbumTagsOntoImages()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var added = await NewService(factory).ApplyAlbumTagsToImagesAsync(TaggedPublishedId);

        added.Should().Be(2);
        var images = await db.AlbumImages.AsNoTracking().Include(i => i.Tags).Where(i => i.AlbumId == TaggedPublishedId).ToListAsync();
        images.Should().AllSatisfy(i => i.Tags.Should().ContainSingle(t => t.LabelId == LabelId));
    }

    [Fact]
    public async Task ApplyAlbumTagsToImagesAsync_IsIdempotent()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;
        var service = NewService(factory);
        await service.ApplyAlbumTagsToImagesAsync(TaggedPublishedId);

        var addedSecondRun = await service.ApplyAlbumTagsToImagesAsync(TaggedPublishedId);

        addedSecondRun.Should().Be(0);
    }

    [Fact]
    public async Task ReserveSlugsAsync_EmptyCandidates_ReturnsEmpty()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var reserved = await NewService(factory).ReserveSlugsAsync([]);

        reserved.Should().BeEmpty();
    }

    [Fact]
    public async Task ReserveSlugsAsync_SuffixesCollisions()
    {
        var (factory, db) = await SeedAsync();
        await using var _ = db;

        var reserved = await NewService(factory).ReserveSlugsAsync(
        [
            new SlugCandidate(0, "brand-new"),
            new SlugCandidate(0, "older-tagged"),
        ]);

        reserved.Should().Equal("brand-new", "older-tagged-2");
    }
}