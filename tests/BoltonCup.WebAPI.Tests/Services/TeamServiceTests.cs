using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class TeamServiceTests
{
    const int TournamentId = 1;
    const int TeamId = 10;

    static BoltonCupDbContext NewContext() =>
        new(new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseInMemoryDatabase($"team-{Guid.NewGuid()}")
            .Options);

    static TeamService NewService(BoltonCupDbContext db)
    {
        var music = new MusicLibraryService(db, Mock.Of<IStorageService>(), Mock.Of<IAssetKeyGenerator>(), Mock.Of<IMusicSearchService>(), new GlobalMusicQueue(db));
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
            new MusicTrack("W1", "Win Anthem", "Artist W", "art-w"),
            new MusicTrack("P1", "Penalty Anthem", "Artist P", "art-p"));

        var team = await db.Teams.SingleAsync(t => t.Id == TeamId);
        team.GoalSongTrackId.Should().NotBeNull();
        team.WinSongTrackId.Should().NotBeNull();
        team.PenaltySongTrackId.Should().NotBeNull();

        var goal = await db.TournamentMusicTracks.SingleAsync(t => t.TrackId == "G1");
        goal.Status.Should().Be(MusicTrackStatus.Pending);
        goal.Source.Should().Be(MusicTrackSource.PlayerRequest);
        goal.IsInBasePool.Should().BeFalse();
        goal.Title.Should().Be("Goal Anthem");
        goal.Artist.Should().Be("Artist A");
        team.GoalSongTrackId.Should().Be(goal.Id);

        var penalty = await db.TournamentMusicTracks.SingleAsync(t => t.TrackId == "P1");
        penalty.Status.Should().Be(MusicTrackStatus.Pending);
        penalty.Source.Should().Be(MusicTrackSource.PlayerRequest);
        team.PenaltySongTrackId.Should().Be(penalty.Id);
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

        await service.UpdateSongsAsync(TeamId, new MusicTrack("G1", "Goal Anthem", "Artist A", null), winSong: null, penaltySong: null);

        db.TournamentMusicTracks.Count(t => t.TournamentId == TournamentId && t.TrackId == "G1").Should().Be(1);
        var team = await db.Teams.SingleAsync(t => t.Id == TeamId);
        team.GoalSongTrackId.Should().Be(99);
    }

    [Fact]
    public async Task UpdateSongsAsync_NullClearsReference_LeavesTrackRowIntact()
    {
        await using var db = await SeedAsync();
        var service = NewService(db);
        await service.UpdateSongsAsync(TeamId, new MusicTrack("G1", "Goal Anthem", "A", null), null, null);
        var goalId = (await db.Teams.SingleAsync(t => t.Id == TeamId)).GoalSongTrackId;
        goalId.Should().NotBeNull();

        await service.UpdateSongsAsync(TeamId, goalSong: null, winSong: null, penaltySong: null);

        var team = await db.Teams.SingleAsync(t => t.Id == TeamId);
        team.GoalSongTrackId.Should().BeNull();
        // The track row is left in place (it may still be wanted as a player request / library item).
        db.TournamentMusicTracks.Any(t => t.Id == goalId).Should().BeTrue();
    }

    [Fact]
    public async Task SetSongTracksAsync_SetsFksToExistingPoolTracks()
    {
        await using var db = await SeedAsync();
        db.TournamentMusicTracks.AddRange(
            new TournamentMusicTrack { Id = 1, TournamentId = TournamentId, Title = "Goal", AudioFileKey = "g" },
            new TournamentMusicTrack { Id = 2, TournamentId = TournamentId, Title = "Win", AudioFileKey = "w" },
            new TournamentMusicTrack { Id = 3, TournamentId = TournamentId, Title = "Penalty", AudioFileKey = "p" });
        await db.SaveChangesAsync();
        var service = NewService(db);

        await service.SetSongTracksAsync(TeamId, 1, 2, 3);

        var team = await db.Teams.SingleAsync(t => t.Id == TeamId);
        team.GoalSongTrackId.Should().Be(1);
        team.WinSongTrackId.Should().Be(2);
        team.PenaltySongTrackId.Should().Be(3);
    }

    [Fact]
    public async Task SetSongTracksAsync_NullClearsSelection()
    {
        await using var db = await SeedAsync();
        db.TournamentMusicTracks.Add(new TournamentMusicTrack { Id = 1, TournamentId = TournamentId, Title = "Goal", AudioFileKey = "g" });
        await db.SaveChangesAsync();
        var service = NewService(db);
        await service.SetSongTracksAsync(TeamId, 1, null, null);

        await service.SetSongTracksAsync(TeamId, null, null, null);

        var team = await db.Teams.SingleAsync(t => t.Id == TeamId);
        team.GoalSongTrackId.Should().BeNull();
        team.WinSongTrackId.Should().BeNull();
        team.PenaltySongTrackId.Should().BeNull();
    }

    [Fact]
    public async Task SetSongTracksAsync_RejectsTrackFromAnotherTournament()
    {
        await using var db = await SeedAsync();
        db.TournamentMusicTracks.Add(new TournamentMusicTrack { Id = 5, TournamentId = 999, Title = "Foreign", AudioFileKey = "f" });
        await db.SaveChangesAsync();
        var service = NewService(db);

        var act = () => service.SetSongTracksAsync(TeamId, 5, null, null);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    static async Task<BoltonCupDbContext> SeedAsync()
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