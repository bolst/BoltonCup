using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class MusicLibraryServiceTests
{
    private const int TournamentId = 1;
    private const int HomeTeamId = 10;
    private const int AwayTeamId = 20;
    private const int GameId = 5;
    private const int AliceId = 100;
    private const int BobId = 200;

    private static BoltonCupDbContext NewContext() =>
        new(new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseInMemoryDatabase($"music-{Guid.NewGuid()}")
            .Options);

    private static MusicLibraryService NewService(BoltonCupDbContext db, IMusicSearchService? search = null) =>
        new(db, Mock.Of<IStorageService>(), Mock.Of<IAssetKeyGenerator>(), search ?? Mock.Of<IMusicSearchService>());

    // A service whose storage + key generator are stubbed so AddTrackAsync (upload) can run.
    private static MusicLibraryService NewUploadService(BoltonCupDbContext db)
    {
        var keyGen = new Mock<IAssetKeyGenerator>();
        keyGen.Setup(k => k.GenerateFinalKey<TournamentMusicTrack>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("final/audio.mp3");
        var storage = new Mock<IStorageService>();
        storage.Setup(s => s.CopyAssetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new MusicLibraryService(db, storage.Object, keyGen.Object, Mock.Of<IMusicSearchService>());
    }

    private static IMusicSearchService SearchReturning(params MusicTrack[] tracks)
    {
        var mock = new Mock<IMusicSearchService>();
        mock.Setup(s => s.GetPlaylistTracksAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracks);
        return mock.Object;
    }

    [Fact]
    public async Task GetGamePlaylistAsync_MatchesRequestsByTrackId_AppendsBasePool_AndReportsMissing()
    {
        await using var db = await SeedAsync();
        var service = NewService(db);

        var result = await service.GetGamePlaylistAsync(GameId);

        // Alice's request (SA) matched to a file first, then the base-pool track.
        result.Tracks.Select(t => t.AudioFileKey).Should().Equal("kSA", "kbase");

        // Bob's request (SB) has no uploaded file -> reported as missing.
        result.Missing.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { PlayerName = "Bob Jones", SongName = "SongB", SongTrackId = "SB" });
    }

    [Fact]
    public async Task GetGamePlaylistAsync_DeDupesRequestThatIsAlsoInBasePool()
    {
        await using var db = await SeedAsync(saTrackIsBasePool: true);
        var service = NewService(db);

        var result = await service.GetGamePlaylistAsync(GameId);

        result.Tracks.Select(t => t.AudioFileKey).Should().Equal("kSA", "kbase");
    }

    [Fact]
    public async Task GetMissingRequestsAsync_ReturnsRequestsWithNoMatchingFile()
    {
        await using var db = await SeedAsync();
        var service = NewService(db);

        var missing = await service.GetMissingRequestsAsync(TournamentId);

        missing.Select(m => m.SongTrackId).Should().Equal("SB");
    }

    [Fact]
    public async Task GetQueueAsync_SyncsPlayerRequests_ShowsOnlyNonDownloaded()
    {
        await using var db = await SeedAsync();
        var service = NewService(db);

        var queue = await service.GetQueueAsync(TournamentId);

        // SB has no file -> Pending in the queue, attributed to the requesting player.
        queue.Should().Contain(q => q.TrackId == "SB" && q.Status == MusicTrackStatus.Pending
            && q.Source == MusicTrackSource.PlayerRequest && q.RequestedByName == "Bob Jones");
        // SA already has a file -> it lives in the library, not the queue.
        queue.Should().NotContain(q => q.TrackId == "SA");
        (await service.GetLibraryAsync(TournamentId)).Should().Contain(t => t.TrackId == "SA");
    }

    [Fact]
    public async Task CancelQueueItemAsync_SetsCancelled_AndSurvivesResync()
    {
        await using var db = await SeedAsync();
        var service = NewService(db);

        var sb = (await service.GetQueueAsync(TournamentId)).Single(q => q.TrackId == "SB");
        await service.CancelQueueItemAsync(TournamentId, sb.Id);

        // Re-reading (which re-syncs) keeps it cancelled and drops it from the pending/missing list.
        var queue = await service.GetQueueAsync(TournamentId);
        queue.Single(q => q.TrackId == "SB").Status.Should().Be(MusicTrackStatus.Cancelled);

        var missing = await service.GetMissingRequestsAsync(TournamentId);
        missing.Select(m => m.SongTrackId).Should().NotContain("SB");
    }

    [Fact]
    public async Task ImportPlaylistAsync_AddsBasePoolPendingItems_AndDeduptes()
    {
        await using var db = await SeedAsync();
        var search = SearchReturning(
            new MusicTrack("PL1", "Playlist Song", "Some Artist", "art"),
            new MusicTrack("SB", "SongB", "B", null)); // SB already requested by a player
        var service = NewService(db, search);

        var count = await service.ImportPlaylistAsync(TournamentId, "https://open.spotify.com/playlist/abc");

        count.Should().Be(1); // only PL1 is new; SB already existed (now promoted to base pool)

        var queue = await service.GetQueueAsync(TournamentId);
        queue.Should().Contain(q => q.TrackId == "PL1" && q.Status == MusicTrackStatus.Pending
            && q.Source == MusicTrackSource.PlaylistImport && q.IsInBasePool);
        queue.Single(q => q.TrackId == "SB").IsInBasePool.Should().BeTrue();
    }

    [Fact]
    public async Task ImportPlaylistAsync_ReactivatesCancelledItem()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, SearchReturning(new MusicTrack("SB", "SongB", "B", null)));

        var sb = (await service.GetQueueAsync(TournamentId)).Single(q => q.TrackId == "SB");
        await service.CancelQueueItemAsync(TournamentId, sb.Id);

        var count = await service.ImportPlaylistAsync(TournamentId, "playlist");

        count.Should().Be(1);
        (await service.GetQueueAsync(TournamentId)).Single(q => q.TrackId == "SB").Status
            .Should().Be(MusicTrackStatus.Pending);
    }

    [Fact]
    public async Task AddTrackAsync_GraduatesPendingRequestInPlace_NoDuplicate()
    {
        await using var db = await SeedAsync();
        var service = NewUploadService(db);

        // Materialize the pending SB request, then "download" it via an upload tagged with the same track id.
        await service.GetQueueAsync(TournamentId);
        var cmd = new AddMusicTrackCommand(
            TournamentId, "temp.mp3", "SongB", "B", "SB", MusicProviderType.Spotify, null, null, IsInBasePool: false);

        var track = await service.AddTrackAsync(cmd);

        track.Status.Should().Be(MusicTrackStatus.Downloaded);
        db.TournamentMusicTracks.Count(t => t.TournamentId == TournamentId && t.TrackId == "SB").Should().Be(1);
        (await service.GetLibraryAsync(TournamentId)).Should().Contain(t => t.TrackId == "SB");
        (await service.GetQueueAsync(TournamentId)).Should().NotContain(q => q.TrackId == "SB");
    }

    private static async Task<BoltonCupDbContext> SeedAsync(bool saTrackIsBasePool = false)
    {
        var db = NewContext();

        db.Tournaments.Add(new Tournament { Id = TournamentId, Name = "Test Cup" });
        db.Accounts.AddRange(Account(AliceId, "Alice", "Smith"), Account(BobId, "Bob", "Jones"));

        db.Players.AddRange(
            new Player { Id = 1, TournamentId = TournamentId, AccountId = AliceId, TeamId = HomeTeamId, JerseyNumber = 7 },
            new Player { Id = 2, TournamentId = TournamentId, AccountId = BobId, TeamId = AwayTeamId, JerseyNumber = 9 });

        db.TournamentPlayerInfos.AddRange(
            new TournamentPlayerInfo { Id = Guid.NewGuid(), TournamentId = TournamentId, AccountId = AliceId, SongTrackId = "SA", SongName = "SongA" },
            new TournamentPlayerInfo { Id = Guid.NewGuid(), TournamentId = TournamentId, AccountId = BobId, SongTrackId = "SB", SongName = "SongB" });

        db.TournamentMusicTracks.AddRange(
            new TournamentMusicTrack { Id = 1, TournamentId = TournamentId, AudioFileKey = "kSA", TrackId = "SA", Title = "SongA", IsInBasePool = saTrackIsBasePool },
            new TournamentMusicTrack { Id = 2, TournamentId = TournamentId, AudioFileKey = "kbase", TrackId = null, Title = "Anthem", IsInBasePool = true });

        db.Games.Add(new Game
        {
            Id = GameId,
            TournamentId = TournamentId,
            GameTime = new DateTime(2026, 6, 22),
            HomeTeamId = HomeTeamId,
            AwayTeamId = AwayTeamId,
        });

        await db.SaveChangesAsync();
        return db;
    }

    private static Account Account(int id, string first, string last) => new()
    {
        Id = id,
        FirstName = first,
        LastName = last,
        Email = $"{first}@example.com",
        Birthday = new DateTime(1990, 1, 1),
    };
}
