using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Values;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class DraftServiceTests
{
    private static BoltonCupDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseInMemoryDatabase($"draft-{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new BoltonCupDbContext(options);
    }

    /// <summary>
    /// Seeds a tournament with <paramref name="teamCount"/> teams and <paramref name="playerCount"/> players,
    /// a draft in the given status with default orderings, one ranking per player, and one pick per player.
    /// </summary>
    private static async Task<(BoltonCupDbContext Db, Draft Draft, List<int> PlayerIds, List<int> TeamIds)> SeedDraftAsync(
        int teamCount,
        int playerCount,
        DraftStatus status,
        DraftType type = DraftType.Standard)
    {
        var db = NewContext();

        var tournament = new Tournament { Id = 1, Name = "Test Cup" };
        db.Tournaments.Add(tournament);

        var teamIds = new List<int>();
        for (var i = 1; i <= teamCount; i++)
        {
            db.Teams.Add(new Team
            {
                Id = i,
                Name = $"Team {i}",
                NameShort = $"T{i}",
                Abbreviation = $"T{i}",
                PrimaryColorHex = "#000000",
                SecondaryColorHex = "#ffffff",
                TournamentId = tournament.Id,
            });
            teamIds.Add(i);
        }

        var playerIds = new List<int>();
        for (var i = 1; i <= playerCount; i++)
        {
            db.Accounts.Add(new Account
            {
                Id = i,
                FirstName = $"First{i}",
                LastName = $"Last{i}",
                Email = $"p{i}@test.com",
                Birthday = new DateTime(1990, 1, 1),
            });
            db.Players.Add(new Player
            {
                Id = i,
                AccountId = i,
                TournamentId = tournament.Id,
                Position = Position.Forward,
            });
            playerIds.Add(i);
        }

        var draft = new Draft
        {
            Id = 1,
            TournamentId = tournament.Id,
            Title = "Test Draft",
            Status = status,
            Type = type,
        };
        db.Drafts.Add(draft);

        for (var i = 0; i < teamCount; i++)
        {
            db.DraftOrders.Add(new DraftOrder
            {
                DraftId = draft.Id,
                TeamId = teamIds[i],
                Pick = i + 1,
            });
        }

        for (var i = 0; i < playerCount; i++)
        {
            db.PlayerDraftRankings.Add(new PlayerDraftRanking
            {
                PlayerId = playerIds[i],
                TournamentId = tournament.Id,
                DraftId = draft.Id,
                DraftRanking = i + 1,
            });
        }

        // one pick per player, snake-aware, mirroring GenerateDraftPicksAsync
        for (var i = 0; i < playerCount; i++)
        {
            var round = i / teamCount + 1;
            var standardRoundPick = i % teamCount + 1;
            var roundPick = type == DraftType.Snake && round % 2 == 0
                ? teamCount - standardRoundPick + 1
                : standardRoundPick;
            db.DraftPicks.Add(new DraftPick
            {
                DraftId = draft.Id,
                OverallPick = i + 1,
                Round = round,
                RoundPick = roundPick,
                TeamId = teamIds[roundPick - 1],
            });
        }

        draft.Teams = teamCount;
        draft.Rounds = (int)Math.Ceiling((double)playerCount / teamCount);

        await db.SaveChangesAsync();
        return (db, draft, playerIds, teamIds);
    }

    [Fact]
    public async Task SetPlayerPoolAsync_ExcludingPlayers_RegeneratesPicksAndRounds()
    {
        var (db, draft, playerIds, _) = await SeedDraftAsync(teamCount: 2, playerCount: 6, DraftStatus.Pending);
        var service = new DraftService(db);

        var excluded = new[] { playerIds[4], playerIds[5] };

        await service.SetPlayerPoolAsync(draft.Id, new SetPlayerPoolCommand(excluded));

        var picks = await db.DraftPicks.Where(p => p.DraftId == draft.Id).ToListAsync();
        picks.Should().HaveCount(4);

        var refreshed = await db.Drafts.SingleAsync(d => d.Id == draft.Id);
        refreshed.Rounds.Should().Be(2); // ceil(4/2)

        var rankings = await db.PlayerDraftRankings.Where(r => r.DraftId == draft.Id).ToListAsync();
        rankings.Where(r => r.IsExcluded).Select(r => r.PlayerId).Should().BeEquivalentTo(excluded);
    }

    [Fact]
    public async Task SetPlayerPoolAsync_PreservesOrderingAndDefaultRanking()
    {
        var (db, draft, playerIds, _) = await SeedDraftAsync(teamCount: 2, playerCount: 6, DraftStatus.Pending);
        draft.DefaultCustomRankingId = 99;
        await db.SaveChangesAsync();
        var service = new DraftService(db);

        var rankingsBefore = await db.PlayerDraftRankings
            .Where(r => r.DraftId == draft.Id)
            .ToDictionaryAsync(r => r.PlayerId, r => r.DraftRanking);

        await service.SetPlayerPoolAsync(draft.Id, new SetPlayerPoolCommand(new[] { playerIds[5] }));

        var refreshed = await db.Drafts.SingleAsync(d => d.Id == draft.Id);
        refreshed.DefaultCustomRankingId.Should().Be(99); // exclusion does not touch the applied ranking

        var rankingsAfter = await db.PlayerDraftRankings.Where(r => r.DraftId == draft.Id).ToListAsync();
        foreach (var r in rankingsAfter)
            r.DraftRanking.Should().Be(rankingsBefore[r.PlayerId]); // ordering untouched
    }

    [Fact]
    public async Task SetPlayerPoolAsync_ReincludingPlayer_RestoresPickCount()
    {
        var (db, draft, playerIds, _) = await SeedDraftAsync(teamCount: 2, playerCount: 6, DraftStatus.Pending);
        var service = new DraftService(db);

        await service.SetPlayerPoolAsync(draft.Id, new SetPlayerPoolCommand(new[] { playerIds[5] }));
        await service.SetPlayerPoolAsync(draft.Id, new SetPlayerPoolCommand(Array.Empty<int>()));

        var picks = await db.DraftPicks.Where(p => p.DraftId == draft.Id).ToListAsync();
        picks.Should().HaveCount(6);
        (await db.PlayerDraftRankings.Where(r => r.DraftId == draft.Id).ToListAsync())
            .Should().OnlyContain(r => !r.IsExcluded);
    }

    [Fact]
    public async Task SetPlayerPoolAsync_WhenNotPending_Throws()
    {
        var (db, draft, playerIds, _) = await SeedDraftAsync(teamCount: 2, playerCount: 4, DraftStatus.InProgress);
        var service = new DraftService(db);

        var act = () => service.SetPlayerPoolAsync(draft.Id, new SetPlayerPoolCommand(new[] { playerIds[0] }));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetPlayerPoolAsync_WhenExcludingUnknownPlayer_Throws()
    {
        var (db, draft, _, _) = await SeedDraftAsync(teamCount: 2, playerCount: 4, DraftStatus.Pending);
        var service = new DraftService(db);

        var act = () => service.SetPlayerPoolAsync(draft.Id, new SetPlayerPoolCommand(new[] { 9999 }));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UndoLastPickAsync_RevertsTrailingAutoPicksAndPrecedingManualPick()
    {
        var (db, draft, playerIds, _) = await SeedDraftAsync(teamCount: 2, playerCount: 6, DraftStatus.InProgress);

        // make the first three picks: manual (1), then two auto (2, 3)
        var picks = await db.DraftPicks.Where(p => p.DraftId == draft.Id).OrderBy(p => p.OverallPick).ToListAsync();
        await MakePickAsync(db, picks[0], playerIds[0], isAuto: false);
        await MakePickAsync(db, picks[1], playerIds[1], isAuto: true);
        await MakePickAsync(db, picks[2], playerIds[2], isAuto: true);

        var service = new DraftService(db);
        var state = await service.UndoLastPickAsync(draft.Id);

        var refreshedPicks = await db.DraftPicks.Where(p => p.DraftId == draft.Id).OrderBy(p => p.OverallPick).ToListAsync();
        refreshedPicks.Take(3).Should().OnlyContain(p => p.PlayerId == null);
        refreshedPicks[0].ClockStartedAt.Should().NotBeNull(); // reopened slot
        refreshedPicks[0].IsAutoPick.Should().BeFalse();

        var revertedRankings = await db.PlayerDraftRankings
            .Where(r => r.DraftId == draft.Id && new[] { playerIds[0], playerIds[1], playerIds[2] }.Contains(r.PlayerId))
            .ToListAsync();
        revertedRankings.Should().OnlyContain(r => r.DraftPickId == null);

        state.NextPick!.OverallPick.Should().Be(1);
    }

    [Fact]
    public async Task UndoLastPickAsync_WhenAllPicksAuto_RevertsAll()
    {
        var (db, draft, playerIds, _) = await SeedDraftAsync(teamCount: 2, playerCount: 6, DraftStatus.InProgress);

        var picks = await db.DraftPicks.Where(p => p.DraftId == draft.Id).OrderBy(p => p.OverallPick).ToListAsync();
        await MakePickAsync(db, picks[0], playerIds[0], isAuto: true);
        await MakePickAsync(db, picks[1], playerIds[1], isAuto: true);

        var service = new DraftService(db);
        await service.UndoLastPickAsync(draft.Id);

        var made = await db.DraftPicks.Where(p => p.DraftId == draft.Id && p.PlayerId != null).ToListAsync();
        made.Should().BeEmpty();
    }

    [Fact]
    public async Task UndoLastPickAsync_WhenCompleted_ReopensToInProgress()
    {
        var (db, draft, playerIds, _) = await SeedDraftAsync(teamCount: 1, playerCount: 2, DraftStatus.Completed);

        var picks = await db.DraftPicks.Where(p => p.DraftId == draft.Id).OrderBy(p => p.OverallPick).ToListAsync();
        await MakePickAsync(db, picks[0], playerIds[0], isAuto: false);
        await MakePickAsync(db, picks[1], playerIds[1], isAuto: true);

        var service = new DraftService(db);
        await service.UndoLastPickAsync(draft.Id);

        var refreshed = await db.Drafts.SingleAsync(d => d.Id == draft.Id);
        refreshed.Status.Should().Be(DraftStatus.InProgress);

        var made = await db.DraftPicks.Where(p => p.DraftId == draft.Id && p.PlayerId != null).ToListAsync();
        made.Should().BeEmpty(); // both reverted: trailing auto + preceding manual
    }

    [Fact]
    public async Task UndoLastPickAsync_WhenNoPicksMade_Throws()
    {
        var (db, draft, _, _) = await SeedDraftAsync(teamCount: 2, playerCount: 4, DraftStatus.InProgress);
        var service = new DraftService(db);

        var act = () => service.UndoLastPickAsync(draft.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static async Task MakePickAsync(BoltonCupDbContext db, DraftPick pick, int playerId, bool isAuto)
    {
        pick.PlayerId = playerId;
        pick.IsAutoPick = isAuto;
        var ranking = await db.PlayerDraftRankings.SingleAsync(r => r.DraftId == pick.DraftId && r.PlayerId == playerId);
        ranking.DraftPickId = pick.Id;
        await db.SaveChangesAsync();
    }
}
