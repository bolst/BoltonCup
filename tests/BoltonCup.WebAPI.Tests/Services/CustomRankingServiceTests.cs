using BoltonCup.Core;
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

        foreach (var (id, name) in new[] { (OwnerId, "Owner Account"), (Gm1Id, "Gm One"), (Gm2Id, "Gm Two"), (NonGmId, "Non Gm") })
        {
            var parts = name.Split(' ');
            db.Accounts.Add(new Account
            {
                Id = id,
                FirstName = parts[0],
                LastName = parts[1],
                Email = $"user{id}@test.com",
                Birthday = new DateTime(1990, 1, 1),
            });
        }

        db.Teams.Add(new Team
        {
            Id = 1, Name = "Team 1", NameShort = "T1", Abbreviation = "T1",
            PrimaryColorHex = "#000000", SecondaryColorHex = "#ffffff",
            TournamentId = TournamentId, GmAccountId = Gm1Id,
        });
        db.Teams.Add(new Team
        {
            Id = 2, Name = "Team 2", NameShort = "T2", Abbreviation = "T2",
            PrimaryColorHex = "#000000", SecondaryColorHex = "#ffffff",
            TournamentId = TournamentId, GmAccountId = Gm2Id,
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
    public async Task GetByIdAsync_IncludesSharedWith()
    {
        await using var db = await SeedAsync();
        var service = new CustomRankingService(db);
        await service.AddShareAsync(RankingId, Gm1Id);

        var ranking = await service.GetByIdAsync(RankingId);

        ranking.Should().NotBeNull();
        ranking!.SharedWith.Select(s => s.SharedWithAccountId).Should().BeEquivalentTo(new[] { Gm1Id });
    }
}
