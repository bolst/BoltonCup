using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class TournamentPlayerInfoServiceTests
{
    private const int TournamentId = 1;
    private const int TeamId = 10;
    private const int GmId = 100;
    private const int PlayerId = 200;

    private static BoltonCupDbContext NewContext() =>
        new(new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseInMemoryDatabase($"playerinfo-{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task GetAsync_ReturnsManagedTeam_WithSongs_ForGm()
    {
        await using var db = await SeedAsync();
        var context = await new TournamentPlayerInfoService(db).GetAsync(TournamentId, GmId);

        context.ManagedTeam.Should().NotBeNull();
        context.ManagedTeam!.TeamId.Should().Be(TeamId);
        context.ManagedTeam.GoalSongTrack.Should().NotBeNull();
        context.ManagedTeam.GoalSongTrack!.TrackId.Should().Be("G1");
        context.ManagedTeam.WinSongTrack.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ReturnsNoManagedTeam_ForNonGm()
    {
        await using var db = await SeedAsync();
        var context = await new TournamentPlayerInfoService(db).GetAsync(TournamentId, PlayerId);

        context.ManagedTeam.Should().BeNull();
    }

    private static async Task<BoltonCupDbContext> SeedAsync()
    {
        var db = NewContext();
        db.Tournaments.Add(new Tournament { Id = TournamentId, Name = "Test Cup" });
        db.Accounts.AddRange(
            Account(GmId, "Gina", "Manager"),
            Account(PlayerId, "Pat", "Player"));

        db.TournamentMusicTracks.Add(new TournamentMusicTrack
        {
            Id = 1,
            TournamentId = TournamentId,
            TrackId = "G1",
            Title = "Goal Anthem",
            Status = MusicTrackStatus.Pending,
            Source = MusicTrackSource.PlayerRequest,
            IsInBasePool = false,
        });

        db.Teams.Add(new Team
        {
            Id = TeamId,
            TournamentId = TournamentId,
            GmAccountId = GmId,
            Name = "Test Team",
            NameShort = "Test",
            Abbreviation = "TST",
            PrimaryColorHex = "#000000",
            SecondaryColorHex = "#ffffff",
            GoalSongTrackId = 1,
        });

        // Both accounts must be players in the tournament for GetAsync to resolve their info.
        db.Players.AddRange(
            new Player { Id = 1, TournamentId = TournamentId, AccountId = GmId, TeamId = TeamId },
            new Player { Id = 2, TournamentId = TournamentId, AccountId = PlayerId, TeamId = TeamId });

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
