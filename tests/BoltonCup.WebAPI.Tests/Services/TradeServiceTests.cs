using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Core.Values;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class TradeServiceTests
{
    private const int TournamentId = 1;
    private const int TeamAId = 10;
    private const int TeamBId = 20;
    private const int GmAAccountId = 100;
    private const int GmBAccountId = 200;
    private const int AdminAccountId = 999;

    private static BoltonCupDbContext NewContext() =>
        new(new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseInMemoryDatabase($"trade-{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private static UserManager<BoltonCupUser> EmptyAdminUserManager()
    {
        var store = new Mock<IUserStore<BoltonCupUser>>();
        var mgr = new Mock<UserManager<BoltonCupUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.GetUsersInRoleAsync(It.IsAny<string>())).ReturnsAsync(new List<BoltonCupUser>());
        return mgr.Object;
    }

    private static TradeService NewService(BoltonCupDbContext db, out Mock<IEmailer> emailer)
    {
        emailer = new Mock<IEmailer>();
        return new TradeService(db, new RosterValidator(), emailer.Object, EmptyAdminUserManager());
    }

    /// <summary>Seeds a tournament with two teams (each with a GM) and N players per team.</summary>
    private static async Task<BoltonCupDbContext> SeedAsync(
        bool tradingOpen = true,
        int playersPerTeam = 5,
        int? skaterLimit = null,
        int? goalieLimit = null)
    {
        var db = NewContext();
        db.Tournaments.Add(new Tournament
        {
            Id = TournamentId,
            Name = "Test Cup",
            IsTradingOpen = tradingOpen,
            SkaterLimit = skaterLimit,
            GoalieLimit = goalieLimit,
        });

        db.Accounts.AddRange(
            Account(GmAAccountId), Account(GmBAccountId),
            Account(AdminAccountId));

        db.Teams.Add(Team(TeamAId, GmAAccountId));
        db.Teams.Add(Team(TeamBId, GmBAccountId));

        var pid = 1;
        foreach (var (teamId, _) in new[] { (TeamAId, GmAAccountId), (TeamBId, GmBAccountId) })
        {
            for (var i = 0; i < playersPerTeam; i++)
            {
                var accountId = 1000 + pid;
                db.Accounts.Add(Account(accountId));
                db.Players.Add(new Player
                {
                    Id = pid,
                    AccountId = accountId,
                    TournamentId = TournamentId,
                    TeamId = teamId,
                    Position = i == 0 ? Position.Goalie : Position.Forward,
                });
                pid++;
            }
        }

        await db.SaveChangesAsync();
        return db;
    }

    private static Account Account(int id) => new()
    {
        Id = id,
        FirstName = $"First{id}",
        LastName = $"Last{id}",
        Email = $"user{id}@test.com",
        Birthday = new DateTime(1990, 1, 1),
    };

    private static Team Team(int id, int gmAccountId) => new()
    {
        Id = id,
        Name = $"Team {id}",
        NameShort = $"T{id}",
        Abbreviation = $"T{id}",
        PrimaryColorHex = "#000000",
        SecondaryColorHex = "#ffffff",
        TournamentId = TournamentId,
        GmAccountId = gmAccountId,
    };

    // Team A forwards are player ids 2..5 (id 1 is a goalie); Team B forwards are 7..10 (id 6 goalie).
    private static CreateTradeCommand Trade(IReadOnlyList<int> fromA, IReadOnlyList<int> fromB) =>
        new(TournamentId, TeamAId, TeamBId, fromA, fromB, Note: null, CreatedByAccountId: GmAAccountId);

    [Fact]
    public async Task Create_HappyPath_PersistsPendingTradeAndEmails()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out var emailer);

        var id = await service.CreateAsync(Trade([2], [7]));

        var trade = await db.Trades.Include(t => t.Players).FirstAsync(t => t.Id == id);
        trade.Status.Should().Be(TradeStatus.Pending);
        trade.Players.Should().HaveCount(2);
        emailer.Verify(e => e.SendTradeCreatedAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<TradeEmailInfo>()), Times.Once);
    }

    [Fact]
    public async Task Create_Giveaway_OneSideEmpty_IsAllowed()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out _);

        var id = await service.CreateAsync(Trade([2, 3], []));

        var trade = await db.Trades.Include(t => t.Players).FirstAsync(t => t.Id == id);
        trade.Players.Should().HaveCount(2);
    }

    [Fact]
    public async Task Create_WhenTradingClosed_Throws()
    {
        await using var db = await SeedAsync(tradingOpen: false);
        var service = NewService(db, out _);

        await Assert.ThrowsAsync<TradingClosedException>(() => service.CreateAsync(Trade([2], [7])));
    }

    [Fact]
    public async Task Create_PlayerNotOnFromTeam_Throws()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out _);

        // player 7 belongs to Team B, not Team A
        await Assert.ThrowsAsync<PlayerNotTradeableException>(() => service.CreateAsync(Trade([7], [])));
    }

    [Fact]
    public async Task Create_GmPlayerIneligible_Throws()
    {
        await using var db = await SeedAsync();
        // Make player 2's account the GM of team A
        var player2 = await db.Players.FirstAsync(p => p.Id == 2);
        var teamA = await db.Teams.FirstAsync(t => t.Id == TeamAId);
        teamA.GmAccountId = player2.AccountId;
        await db.SaveChangesAsync();

        var service = NewService(db, out _);
        await Assert.ThrowsAsync<PlayerNotTradeableException>(() => service.CreateAsync(Trade([2], [7])));
    }

    [Fact]
    public async Task Create_LockedPlayer_Throws()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out _);
        await service.CreateAsync(Trade([2], [7]));

        // player 2 is now locked in a pending trade
        await Assert.ThrowsAsync<PlayerLockedException>(() => service.CreateAsync(Trade([2], [8])));
    }

    [Fact]
    public async Task Decline_FreesPlayersImmediately()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out _);
        var id = await service.CreateAsync(Trade([2], [7]));

        await service.DeclineAsync(id, GmBAccountId);

        (await db.Trades.FirstAsync(t => t.Id == id)).Status.Should().Be(TradeStatus.Declined);
        // player 2 can now be traded again
        var id2 = await service.CreateAsync(Trade([2], [8]));
        id2.Should().BePositive();
    }

    [Fact]
    public async Task Accept_FromPending_SetsAccepted()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out var emailer);
        var id = await service.CreateAsync(Trade([2], [7]));

        await service.AcceptAsync(id, GmBAccountId);

        var trade = await db.Trades.FirstAsync(t => t.Id == id);
        trade.Status.Should().Be(TradeStatus.Accepted);
        trade.RespondedByAccountId.Should().Be(GmBAccountId);
        emailer.Verify(e => e.SendTradeAcceptedAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<TradeEmailInfo>()), Times.Once);
    }

    [Fact]
    public async Task Approve_FromPending_Throws()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out _);
        var id = await service.CreateAsync(Trade([2], [7]));

        await Assert.ThrowsAsync<InvalidTradeStateException>(() => service.ApproveAsync(id, AdminAccountId));
    }

    [Fact]
    public async Task Approve_FromAccepted_SwapsRostersAndEmailsPlayers()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out var emailer);
        var id = await service.CreateAsync(Trade([2], [7]));
        await service.AcceptAsync(id, GmBAccountId);

        await service.ApproveAsync(id, AdminAccountId);

        (await db.Players.FirstAsync(p => p.Id == 2)).TeamId.Should().Be(TeamBId);
        (await db.Players.FirstAsync(p => p.Id == 7)).TeamId.Should().Be(TeamAId);
        (await db.Trades.FirstAsync(t => t.Id == id)).Status.Should().Be(TradeStatus.Approved);
        emailer.Verify(e => e.SendTradeApprovedAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<TradeEmailInfo>()), Times.Once);
    }

    [Fact]
    public async Task Approve_WouldExceedSkaterLimit_Throws()
    {
        // Team A starts with 4 skaters + 1 goalie (limit 4). A giveaway from B to A pushes A to 5 skaters.
        await using var db = await SeedAsync(skaterLimit: 4);
        var service = NewService(db, out _);
        var id = await service.CreateAsync(Trade([], [7])); // B sends a forward to A, A sends nothing
        await service.AcceptAsync(id, GmBAccountId);

        await Assert.ThrowsAsync<InvalidRosterException>(() => service.ApproveAsync(id, AdminAccountId));
    }

    [Fact]
    public async Task Cancel_ByAdmin_FromAccepted_Succeeds()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out _);
        var id = await service.CreateAsync(Trade([2], [7]));
        await service.AcceptAsync(id, GmBAccountId);

        await service.CancelAsync(id, AdminAccountId, isAdmin: true);

        (await db.Trades.FirstAsync(t => t.Id == id)).Status.Should().Be(TradeStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_ByGm_FromAccepted_Throws()
    {
        await using var db = await SeedAsync();
        var service = NewService(db, out _);
        var id = await service.CreateAsync(Trade([2], [7]));
        await service.AcceptAsync(id, GmBAccountId);

        await Assert.ThrowsAsync<InvalidTradeStateException>(() => service.CancelAsync(id, GmAAccountId, isAdmin: false));
    }
}
