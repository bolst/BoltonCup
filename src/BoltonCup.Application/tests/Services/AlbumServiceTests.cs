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

    static AlbumService NewService(IDbContextFactory<BoltonCupDbContext> factory, IStorageService? storage = null, IAssetKeyGenerator? keys = null, IAssetStager? stager = null)
        => new(factory, storage ?? Mock.Of<IStorageService>(), keys ?? Mock.Of<IAssetKeyGenerator>(), stager ?? Mock.Of<IAssetStager>());

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

    const int TargetId = 50;

    static async Task<(IDbContextFactory<BoltonCupDbContext> Factory, BoltonCupDbContext Db)> SeedTaggedImagesAsync()
    {
        var factory = NewFactory();
        var db = await factory.CreateDbContextAsync();
        db.Albums.AddRange(
            new Album
            {
                Id = 40,
                Title = "Newer",
                Slug = "newer",
                IsPublished = true,
                OccurredAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                Images =
                [
                    new AlbumImage
                    {
                        Id = 400,
                        Key = "media/album/40/images/newer.webp",
                        SortOrder = 1,
                        Tags = [new AlbumImageTag { AccountId = TargetId }],
                    },
                    new AlbumImage
                    {
                        Id = 401,
                        Key = "media/album/40/images/untagged.webp",
                        SortOrder = 2,
                    },
                ],
            },
            new Album
            {
                Id = 41,
                Title = "Older",
                Slug = "older",
                IsPublished = true,
                OccurredAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                Images =
                [
                    new AlbumImage
                    {
                        Id = 410,
                        Key = "media/album/41/images/older.webp",
                        SortOrder = 1,
                        Tags = [new AlbumImageTag { AccountId = TargetId }],
                    },
                    new AlbumImage
                    {
                        Id = 411,
                        Key = "media/album/41/images/team.webp",
                        SortOrder = 2,
                        Tags = [new AlbumImageTag { TeamId = TargetId }],
                    },
                    new AlbumImage
                    {
                        Id = 412,
                        Key = "media/album/41/images/label.webp",
                        SortOrder = 3,
                        Tags = [new AlbumImageTag { LabelId = TargetId }],
                    },
                ],
            },
            new Album
            {
                Id = 42,
                Title = "Draft",
                Slug = "draft-images",
                IsPublished = false,
                Images =
                [
                    new AlbumImage
                    {
                        Id = 420,
                        Key = "media/album/42/images/draft.webp",
                        SortOrder = 1,
                        Tags = [new AlbumImageTag { AccountId = TargetId }],
                    },
                ],
            });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return (factory, db);
    }

    [Theory]
    [InlineData(TagTargetType.Account, 400, 410)]
    [InlineData(TagTargetType.Team, 411)]
    [InlineData(TagTargetType.Label, 412)]
    public async Task GetPublishedImagesByTagAsync_FiltersByTagType(TagTargetType type, params int[] expectedIds)
    {
        var (factory, db) = await SeedTaggedImagesAsync();
        await using var _ = db;

        var result = await NewService(factory).GetPublishedImagesByTagAsync(new GetAlbumImagesQuery { TagType = type, TargetId = TargetId });

        result.Items.Select(i => i.Id).Should().Equal(expectedIds);
    }

    [Fact]
    public async Task GetPublishedImagesByTagAsync_ExcludesUnpublishedAlbums()
    {
        var (factory, db) = await SeedTaggedImagesAsync();
        await using var _ = db;

        var result = await NewService(factory).GetPublishedImagesByTagAsync(new GetAlbumImagesQuery { TagType = TagTargetType.Account, TargetId = TargetId });

        result.Items.Select(i => i.Id).Should().NotContain(420);
    }

    [Fact]
    public async Task GetPublishedImagesByTagAsync_OrdersByNewestAlbumThenSortOrder()
    {
        var (factory, db) = await SeedTaggedImagesAsync();
        await using var _ = db;

        var result = await NewService(factory).GetPublishedImagesByTagAsync(new GetAlbumImagesQuery
        {
            TagType = TagTargetType.Account,
            TargetId = TargetId,
        });

        result.Items.Select(i => i.Id).Should().Equal(400, 410);
    }

    [Fact]
    public async Task GetPublishedImagesByTagAsync_Paginates()
    {
        var (factory, db) = await SeedTaggedImagesAsync();
        await using var _ = db;
        var service = NewService(factory);

        var page1 = await service.GetPublishedImagesByTagAsync(new GetAlbumImagesQuery { TagType = TagTargetType.Account, TargetId = TargetId, Page = 1, Size = 1 });
        var page2 = await service.GetPublishedImagesByTagAsync(new GetAlbumImagesQuery { TagType = TagTargetType.Account, TargetId = TargetId, Page = 2, Size = 1 });

        page1.Total.Should().Be(2);
        page1.Items.Select(i => i.Id).Should().Equal(400);
        page2.Items.Select(i => i.Id).Should().Equal(410);
    }

    [Fact]
    public async Task StagePublishedImageAsync_ReturnsNull_WhenImageMissing()
    {
        var (factory, db) = await SeedTaggedImagesAsync();
        await using var _ = db;

        var key = await NewService(factory).StagePublishedImageAsync(999);

        key.Should().BeNull();
    }

    [Fact]
    public async Task StagePublishedImageAsync_ReturnsNull_WhenAlbumUnpublished()
    {
        var (factory, db) = await SeedTaggedImagesAsync();
        await using var _ = db;

        var key = await NewService(factory).StagePublishedImageAsync(420);

        key.Should().BeNull();
    }

    [Fact]
    public async Task StagePublishedImageAsync_CopiesPublishedImageToTempKey()
    {
        var (factory, db) = await SeedTaggedImagesAsync();
        await using var _ = db;
        var stager = new Mock<IAssetStager>();
        stager.Setup(s => s.CopyToTempAsync("media/album/40/images/newer.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync("temp_uploads/new-key.webp");

        var key = await NewService(factory, stager: stager.Object).StagePublishedImageAsync(400);

        key.Should().Be("temp_uploads/new-key.webp");
        stager.Verify(s => s.CopyToTempAsync("media/album/40/images/newer.webp", It.IsAny<CancellationToken>()), Times.Once);
    }
}