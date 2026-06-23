using BoltonCup.Core;
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

    private static MusicLibraryService NewService(BoltonCupDbContext db) =>
        new(db, Mock.Of<IStorageService>(), Mock.Of<IAssetKeyGenerator>());

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
