using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class TeamServiceTests
{
    private const int TournamentId = 1;
    private const int TeamId = 10;

    private static BoltonCupDbContext NewContext() =>
        new(new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseInMemoryDatabase($"team-{Guid.NewGuid()}")
            .Options);

    private static TeamService NewService(BoltonCupDbContext db)
    {
        var music = new MusicLibraryService(db, Mock.Of<IStorageService>(), Mock.Of<IAssetKeyGenerator>(), Mock.Of<IMusicSearchService>());
        return new TeamService(db, Mock.Of<IStorageService>(), Mock.Of<IAssetKeyGenerator>(), music);
    }

    [Fact]
    public async Task UpdateSongsAsync_RegistersPendingTracks_AndPointsTeamAtThem()
    {
        await using var db = await SeedAsync();
        var service = NewService(db);

        await service.UpdateSongsAsync(
            TeamId,
            new MusicTrack("G1", "Goal Anthem", "Artist A", "art-g"),
            new MusicTrack("W1", "Win Anthem", "Artist W", "art-w"));

        var team = await db.Teams.SingleAsync(t => t.Id == TeamId);
        team.GoalSongTrackId.Should().NotBeNull();
        team.WinSongTrackId.Should().NotBeNull();

        var goal = await db.TournamentMusicTracks.SingleAsync(t => t.TrackId == "G1");
        goal.Status.Should().Be(MusicTrackStatus.Pending);
        goal.Source.Should().Be(MusicTrackSource.PlayerRequest);
        goal.IsInBasePool.Should().BeFalse();
        goal.Title.Should().Be("Goal Anthem");
        goal.Artist.Should().Be("Artist A");
        team.GoalSongTrackId.Should().Be(goal.Id);
    }

    [Fact]
    public async Task UpdateSongsAsync_ReusesExistingTrack_NoDuplicate()
    {
        await using var db = await SeedAsync();
        // A player already requested the same song the GM now picks as the goal song.
        db.TournamentMusicTracks.Add(new TournamentMusicTrack
        {
            Id = 99,
            TournamentId = TournamentId,
            TrackId = "G1",
            Title = "Goal Anthem",
            Status = MusicTrackStatus.Pending,
            Source = MusicTrackSource.PlayerRequest,
            IsInBasePool = false,
        });
        await db.SaveChangesAsync();
        var service = NewService(db);

        await service.UpdateSongsAsync(TeamId, new MusicTrack("G1", "Goal Anthem", "Artist A", null), winSong: null);

        db.TournamentMusicTracks.Count(t => t.TournamentId == TournamentId && t.TrackId == "G1").Should().Be(1);
        var team = await db.Teams.SingleAsync(t => t.Id == TeamId);
        team.GoalSongTrackId.Should().Be(99);
    }

    [Fact]
    public async Task UpdateSongsAsync_NullClearsReference_LeavesTrackRowIntact()
    {
        await using var db = await SeedAsync();
        var service = NewService(db);
        await service.UpdateSongsAsync(TeamId, new MusicTrack("G1", "Goal Anthem", "A", null), null);
        var goalId = (await db.Teams.SingleAsync(t => t.Id == TeamId)).GoalSongTrackId;
        goalId.Should().NotBeNull();

        await service.UpdateSongsAsync(TeamId, goalSong: null, winSong: null);

        var team = await db.Teams.SingleAsync(t => t.Id == TeamId);
        team.GoalSongTrackId.Should().BeNull();
        // The track row is left in place (it may still be wanted as a player request / library item).
        db.TournamentMusicTracks.Any(t => t.Id == goalId).Should().BeTrue();
    }

    private static async Task<BoltonCupDbContext> SeedAsync()
    {
        var db = NewContext();
        db.Tournaments.Add(new Tournament { Id = TournamentId, Name = "Test Cup" });
        db.Teams.Add(new Team
        {
            Id = TeamId,
            TournamentId = TournamentId,
            Name = "Test Team",
            NameShort = "Test",
            Abbreviation = "TST",
            PrimaryColorHex = "#000000",
            SecondaryColorHex = "#ffffff",
        });
        await db.SaveChangesAsync();
        return db;
    }
}
