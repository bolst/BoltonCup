using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class CustomRankingServiceTests
{
    private const int OwnerId = 1;
    private const int Gm1Id = 2;
    private const int Gm2Id = 3;
    private const int NonGmId = 4;
    private const int TournamentId = 1;
    private const int RankingId = 1;

    private static BoltonCupDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseInMemoryDatabase($"custom-ranking-{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new BoltonCupDbContext(options);
    }

    /// <summary>
    /// Seeds tournament 1 with two GM teams (GMs = accounts 2 and 3), an owner (1) and a non-GM account (4),
    /// and a custom ranking (id 1) owned by the owner.
    /// </summary>
    private static async Task<BoltonCupDbContext> SeedAsync()
    {
        var db = NewContext();

        db.Tournaments.Add(new Tournament { Id = TournamentId, Name = "Test Cup" });

        var accountsById = new Dictionary<int, Account>();
        foreach (var (id, name) in new[] { (OwnerId, "Owner Account"), (Gm1Id, "Gm One"), (Gm2Id, "Gm Two"), (NonGmId, "Non Gm") })
        {
            var parts = name.Split(' ');
            var account = new Account
            {
                Id = id,
                FirstName = parts[0],
                LastName = parts[1],
                Email = $"user{id}@test.com",
                Birthday = new DateTime(1990, 1, 1),
            };
            accountsById[id] = account;
            db.Accounts.Add(account);
        }

        db.Teams.Add(new Team
        {
            Id = 1, Name = "Team 1", NameShort = "T1", Abbreviation = "T1",
            PrimaryColorHex = "#000000", SecondaryColorHex = "#ffffff",
            TournamentId = TournamentId, GeneralManagers = [accountsById[Gm1Id]],
        });
        db.Teams.Add(new Team
        {
            Id = 2, Name = "Team 2", NameShort = "T2", Abbreviation = "T2",
            PrimaryColorHex = "#000000", SecondaryColorHex = "#ffffff",
            TournamentId = TournamentId, GeneralManagers = [accountsById[Gm2Id]],
        });

        db.CustomRankings.Add(new CustomRanking
        {
            Id = RankingId,
            AccountId = OwnerId,
            TournamentId = TournamentId,
            Title = "My Board",
        });

        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task AddShareAsync_AllowsTournamentGm_AndIsIdempotent()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);

        await service.AddShareAsync(RankingId, Gm1Id);
        await service.AddShareAsync(RankingId, Gm1Id); // second add is a no-op

        var shares = await db.CustomRankingShares.Where(s => s.CustomRankingId == RankingId).ToListAsync();
        shares.Should().ContainSingle().Which.SharedWithAccountId.Should().Be(Gm1Id);
    }

    [Fact]
    public async Task AddShareAsync_RejectsNonGm()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);

        var act = () => service.AddShareAsync(RankingId, NonGmId);

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await db.CustomRankingShares.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task AddShareAsync_RejectsOwner()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);

        var act = () => service.AddShareAsync(RankingId, OwnerId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SearchInvitableGmsAsync_ReturnsOnlyGms_ExcludingOwnerAndAlreadyShared()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);

        var before = await service.SearchInvitableGmsAsync(RankingId, query: null);
        before.Select(c => c.AccountId).Should().BeEquivalentTo(new[] { Gm1Id, Gm2Id });

        await service.AddShareAsync(RankingId, Gm1Id);

        var after = await service.SearchInvitableGmsAsync(RankingId, query: null);
        after.Select(c => c.AccountId).Should().BeEquivalentTo(new[] { Gm2Id });
    }

    [Fact]
    public async Task GetSharedWithAccountAsync_ReturnsRankingsSharedWithAccount()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);

        await service.AddShareAsync(RankingId, Gm1Id);

        var sharedWithGm1 = await service.GetSharedWithAccountAsync(Gm1Id);
        sharedWithGm1.Select(r => r.Id).Should().BeEquivalentTo(new[] { RankingId });

        var sharedWithGm2 = await service.GetSharedWithAccountAsync(Gm2Id);
        sharedWithGm2.Should().BeEmpty();
    }

    [Fact]
    public async Task CloneAsync_CopiesPlayers_WithNewOwnerAndCopyTitle()
    {
        await using var db = await SeedAsync();
        db.CustomRankingPlayers.AddRange(
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 10, Rank = 1, GamesPlayed = 5, TotalPoints = 12 },
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 11, Rank = 2, GamesPlayed = 4, TotalPoints = 8 },
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 12, Rank = 3, GamesPlayed = 3, TotalPoints = 3 });
        await db.SaveChangesAsync();
        var service = new CustomRankingService(db);

        var newId = await service.CloneAsync(RankingId, Gm1Id);

        newId.Should().NotBe(RankingId);
        var clone = await db.CustomRankings
            .Include(r => r.Players)
            .FirstAsync(r => r.Id == newId);

        clone.AccountId.Should().Be(Gm1Id);
        clone.TournamentId.Should().Be(TournamentId);
        clone.Title.Should().Be("Copy of My Board");
        clone.Players.Select(p => new { p.PlayerId, p.Rank, p.GamesPlayed, p.TotalPoints })
            .Should().BeEquivalentTo(new[]
            {
                new { PlayerId = 10, Rank = 1, GamesPlayed = 5, TotalPoints = 12 },
                new { PlayerId = 11, Rank = 2, GamesPlayed = 4, TotalPoints = 8 },
                new { PlayerId = 12, Rank = 3, GamesPlayed = 3, TotalPoints = 3 },
            });
        clone.Players.Select(p => p.Id).Should().NotContain(0);
    }

    [Fact]
    public async Task CloneAsync_UsesProvidedTitle_WhenGiven()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);

        var newId = await service.CloneAsync(RankingId, Gm1Id, "My Draft Plan");

        var clone = await db.CustomRankings.FirstAsync(r => r.Id == newId);
        clone.Title.Should().Be("My Draft Plan");
    }

    [Fact]
    public async Task CloneAsync_FallsBackToCopyTitle_WhenBlank()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);

        var newId = await service.CloneAsync(RankingId, Gm1Id, "   ");

        var clone = await db.CustomRankings.FirstAsync(r => r.Id == newId);
        clone.Title.Should().Be("Copy of My Board");
    }

    [Fact]
    public async Task CloneAsync_DoesNotCopyShares()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);
        await service.AddShareAsync(RankingId, Gm2Id);

        var newId = await service.CloneAsync(RankingId, Gm1Id);

        (await db.CustomRankingShares.AnyAsync(s => s.CustomRankingId == newId)).Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_IncludesSharedWith()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);
        await service.AddShareAsync(RankingId, Gm1Id);

        var ranking = await service.GetByIdAsync(RankingId);

        ranking.Should().NotBeNull();
        ranking!.SharedWith.Select(s => s.SharedWithAccountId).Should().BeEquivalentTo(new[] { Gm1Id });
    }

    [Fact]
    public async Task ReconcileAsync_AutoRanksNewPlayer_ByPointsPerGame()
    {
        await using var db = await SeedAsync();
        // Existing ranking: a high-PPG and a low-PPG player, both still in the pool.
        AddPoolPlayer(db, playerId: 10, accountId: 10);
        AddPoolPlayer(db, playerId: 12, accountId: 12);
        db.CustomRankingPlayers.AddRange(
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 10, Rank = 1, GamesPlayed = 1, TotalPoints = 10 }, // PPG 10
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 12, Rank = 2, GamesPlayed = 1, TotalPoints = 2 });  // PPG 2

        // New registrant with PPG 5 (one game, five points) should slot between the two.
        AddPoolPlayer(db, playerId: 11, accountId: 11);
        db.SkaterStats.Add(new SkaterStat
        {
            GameId = 1, PlayerId = 11, AccountId = 11,
            GamesPlayed = 1, Goals = 5, Assists = 0, Points = 5, PenaltyMinutes = 0,
            FirstName = "Pool", LastName = "Player11", Position = null, JerseyNumber = null,
            Birthday = new DateTime(1990, 1, 1), ProfilePicture = null,
        });
        await db.SaveChangesAsync();
        var service = new CustomRankingService(db);

        var stale = await service.ReconcileAsync(RankingId);

        stale.Should().BeEmpty();
        var ranks = await db.CustomRankingPlayers
            .Where(p => p.CustomRankingId == RankingId)
            .ToDictionaryAsync(p => p.PlayerId, p => p.Rank);
        ranks.Should().BeEquivalentTo(new Dictionary<int, int> { [10] = 1, [11] = 2, [12] = 3 });
    }

    [Fact]
    public async Task ReconcileAsync_ReportsPlayersNoLongerInPool_AsStale()
    {
        await using var db = await SeedAsync();
        // Player 99 is in the ranking but not in the tournament pool.
        db.CustomRankingPlayers.Add(new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 99, Rank = 1 });
        AddPoolPlayer(db, playerId: 10, accountId: 10);
        await db.SaveChangesAsync();
        var service = new CustomRankingService(db);

        var stale = await service.ReconcileAsync(RankingId);

        stale.Should().BeEquivalentTo(new[] { 99 });
        var players = await db.CustomRankingPlayers.Where(p => p.CustomRankingId == RankingId).ToListAsync();
        players.Select(p => p.PlayerId).Should().BeEquivalentTo(new[] { 99, 10 });
    }

    [Fact]
    public async Task UpdateAsync_RanksListedPlayers_AndAppendsOmitted()
    {
        await using var db = await SeedAsync();
        db.CustomRankingPlayers.AddRange(
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 10, Rank = 1 },
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 11, Rank = 2 },
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 12, Rank = 3 });
        await db.SaveChangesAsync();
        var service = new CustomRankingService(db);

        // Caller reorders two players; the omitted one (11) keeps its place after them, then all renumber.
        await service.UpdateAsync(RankingId, new UpdateCustomRankingCommand(null, new[] { 12, 10 }));

        var ranks = await db.CustomRankingPlayers
            .Where(p => p.CustomRankingId == RankingId)
            .ToDictionaryAsync(p => p.PlayerId, p => p.Rank);
        ranks.Should().BeEquivalentTo(new Dictionary<int, int> { [12] = 1, [10] = 2, [11] = 3 });
    }

    [Fact]
    public async Task UpdateAsync_RejectsUnknownPlayer()
    {
        await using var db = await SeedAsync();
        db.CustomRankingPlayers.Add(new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 10, Rank = 1 });
        await db.SaveChangesAsync();
        var service = new CustomRankingService(db);

        var act = () => service.UpdateAsync(RankingId, new UpdateCustomRankingCommand(null, new[] { 10, 99 }));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateAsync_RejectsDuplicatePlayer()
    {
        await using var db = await SeedAsync();
        db.CustomRankingPlayers.Add(new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 10, Rank = 1 });
        await db.SaveChangesAsync();
        var service = new CustomRankingService(db);

        var act = () => service.UpdateAsync(RankingId, new UpdateCustomRankingCommand(null, new[] { 10, 10 }));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RemovePlayerAsync_DeletesOnlyThatPlayer()
    {
        await using var db = await SeedAsync();
        db.CustomRankingPlayers.AddRange(
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 10, Rank = 1 },
            new CustomRankingPlayer { CustomRankingId = RankingId, PlayerId = 11, Rank = 2 });
        await db.SaveChangesAsync();
        var service = new CustomRankingService(db);

        await service.RemovePlayerAsync(RankingId, 10);

        var players = await db.CustomRankingPlayers.Where(p => p.CustomRankingId == RankingId).ToListAsync();
        players.Select(p => p.PlayerId).Should().BeEquivalentTo(new[] { 11 });
    }

    private static void AddPoolPlayer(BoltonCupDbContext db, int playerId, int accountId)
    {
        db.Accounts.Add(new Account
        {
            Id = accountId,
            FirstName = "Pool",
            LastName = $"Player{playerId}",
            Email = $"pool{playerId}@test.com",
            Birthday = new DateTime(1990, 1, 1),
        });
        db.Players.Add(new Player { Id = playerId, AccountId = accountId, TournamentId = TournamentId });
    }
}
