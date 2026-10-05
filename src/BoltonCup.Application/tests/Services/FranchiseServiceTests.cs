using BoltonCup.Application.Services;
using BoltonCup.Application.Tests.Helpers;
using BoltonCup.Application.Tests.Telemetry;
using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using BoltonCup.Persistence.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static BoltonCup.Application.Tests.Helpers.TestData;

namespace BoltonCup.Application.Tests.Services;

// Atomicity: merge and move each call SaveChangesAsync once, which EF Core runs in one transaction on Npgsql.
// InMemory has no transactions, so the "changes nothing" tests cover validation failures before the save.
public class FranchiseServiceTests
{
    readonly IDbContextFactory<BoltonCupDbContext> _factory = new TestDbContextFactory();

    FranchiseService NewService(ITelemetry? telemetry = null, IAssetKeyGenerator? keyGenerator = null) => new(
        _factory,
        Mock.Of<IStorageService>(),
        keyGenerator ?? Mock.Of<IAssetKeyGenerator>(),
        telemetry ?? TestTelemetry.Instance);

    static Tournament Tournament(int id, int? winningTeamId = null) => new()
    {
        Id = id,
        Name = $"Cup {id}",
        StartDate = new DateTime(2020 + id, 6, 1),
        WinningTeamId = winningTeamId,
    };

    static Account Account(int id, string? lastName = null) => new()
    {
        Id = id,
        FirstName = $"First{id}",
        LastName = lastName ?? $"Last{id}",
        Email = $"a{id}@example.com",
        Birthday = new DateTime(1990, 1, 1),
    };

    static SkaterStat Skater(int gameId, int accountId, int teamId, int tournamentId, int goals, int assists, string? lastName = null, DateTime? gameTime = null) => new()
    {
        GameId = gameId,
        PlayerId = accountId,
        AccountId = accountId,
        TeamId = teamId,
        TournamentId = tournamentId,
        GameTime = gameTime ?? new DateTime(2020 + tournamentId, 6, 1),
        GamesPlayed = 1,
        Goals = goals,
        Assists = assists,
        Points = goals + assists,
        PenaltyMinutes = 0,
        FirstName = $"First{accountId}",
        LastName = lastName ?? $"Last{accountId}",
        Position = null,
        JerseyNumber = null,
        Birthday = new DateTime(1990, 1, 1),
        ProfilePicture = null,
    };

    static GoalieStat Goalie(int gameId, int accountId, int teamId, int tournamentId, int wins, int goalsAgainst, int shotsAgainst, string? lastName = null) => new()
    {
        GameId = gameId,
        PlayerId = accountId,
        AccountId = accountId,
        TeamId = teamId,
        TournamentId = tournamentId,
        GameTime = new DateTime(2020 + tournamentId, 6, 1),
        GamesPlayed = 1,
        Wins = wins,
        GoalsAgainst = goalsAgainst,
        ShotsAgainst = shotsAgainst,
        Saves = shotsAgainst - goalsAgainst,
        Shutouts = goalsAgainst == 0 && wins == 1 ? 1 : 0,
        SavePercentage = 0,
        GoalsAgainstAverage = 0,
        Goals = 0,
        Assists = 0,
        Points = 0,
        PenaltyMinutes = 0,
        FirstName = $"First{accountId}",
        LastName = lastName ?? $"Last{accountId}",
        Position = null,
        JerseyNumber = null,
        Birthday = new DateTime(1990, 1, 1),
        ProfilePicture = null,
    };

    async Task SeedAsync(Action<BoltonCupDbContext> seed)
    {
        await using var db = await _factory.CreateDbContextAsync();
        seed(db);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_SortsByTitlesDescending_ThenName()
    {
        await SeedAsync(db =>
        {
            db.Franchises.AddRange(Franchise(1, "Zebras"), Franchise(2, "Aardvarks"), Franchise(3, "bears"), Franchise(4, "Cats"));
            db.Teams.AddRange(
                Team(10, 1, 1), Team(20, 1, 2),
                Team(11, 2, 1),
                Team(30, 3, 3),
                Team(40, 4, 4));
            db.Tournaments.AddRange(Tournament(1, 10), Tournament(2, 20), Tournament(3, 30), Tournament(4, 40));
        });

        var all = await NewService().GetAllAsync();

        all.Select(s => s.Franchise.Name).Should().Equal("Zebras", "bears", "Cats", "Aardvarks");
        all.Select(s => s.TitleCount).Should().Equal(2, 1, 1, 0);
        all.Single(s => s.Franchise.Id == 1).SeasonCount.Should().Be(2);
    }

    [Fact]
    public async Task Titles_CountDistinctTournamentsWonByAnyFranchiseTeam()
    {
        // Two different season teams of franchise 1 won tournaments 1 and 2; franchise 2 won tournament 3.
        await SeedAsync(db =>
        {
            db.Franchises.AddRange(Franchise(1), Franchise(2));
            db.Teams.AddRange(Team(10, 1, 1), Team(20, 1, 2), Team(21, 1, 2), Team(30, 2, 3));
            db.Tournaments.AddRange(Tournament(1, 10), Tournament(2, 21), Tournament(3, 30));
        });
        var service = NewService();

        var detail = await service.GetDetailBySlugAsync("franchise-1");
        var summary = (await service.GetAllAsync()).Single(s => s.Franchise.Id == 1);

        detail!.Titles.Select(t => t.Id).Should().Equal(1, 2);
        summary.TitleCount.Should().Be(2);
    }

    [Fact]
    public async Task Titles_IgnoreTournamentsWithoutWinner()
    {
        await SeedAsync(db =>
        {
            db.Franchises.Add(Franchise(1));
            db.Teams.AddRange(Team(10, 1, 1), Team(20, 1, 2));
            db.Tournaments.AddRange(Tournament(1, 10), Tournament(2));
        });

        var detail = await NewService().GetDetailBySlugAsync("franchise-1");

        detail!.Titles.Select(t => t.Id).Should().Equal(1);
        detail.Seasons.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDetailBySlug_ReturnsNull_WhenMissing()
    {
        (await NewService().GetDetailBySlugAsync("missing")).Should().BeNull();
    }

    [Fact]
    public async Task GetDetail_SeasonRows_HaveChampionFlagAndGms()
    {
        await SeedAsync(db =>
        {
            db.Franchises.AddRange(Franchise(1), Franchise(2));
            db.Accounts.AddRange(Account(1), Account(2));
            db.Tournaments.AddRange(Tournament(1, 10), Tournament(2, 99));
            var season1 = Team(10, 1, 1, "Old Name");
            season1.GeneralManagers.Add(db.Accounts.Local.Single(a => a.Id == 1));
            var season2 = Team(20, 1, 2, "New Name");
            season2.GeneralManagers.Add(db.Accounts.Local.Single(a => a.Id == 2));
            db.Teams.AddRange(season1, season2, Team(99, 2, 2), Team(90, 2, 1));
            db.Games.AddRange(
                Game(1, homeId: 10, awayId: 90, homeGoals: 2, awayGoals: 1, tournamentId: 1),
                Game(2, homeId: 99, awayId: 20, homeGoals: 3, awayGoals: 0, tournamentId: 2));
        });

        var detail = await NewService().GetDetailBySlugAsync("franchise-1");

        detail!.Seasons.Select(s => s.Team.Id).Should().Equal(20, 10); // newest tournament first
        var latest = detail.Seasons[0];
        latest.Team.Name.Should().Be("New Name");
        latest.Tournament!.Id.Should().Be(2);
        latest.IsChampion.Should().BeFalse();
        latest.GeneralManagers.Select(a => a.Id).Should().Equal(2);
        latest.Record.Should().Be(new FranchiseRecord(1, 0, 1, 0, 0, 3));
        var first = detail.Seasons[1];
        first.IsChampion.Should().BeTrue();
        first.GeneralManagers.Select(a => a.Id).Should().Equal(1);
        first.Record.Should().Be(new FranchiseRecord(1, 1, 0, 0, 2, 1));
        detail.AllTime.Should().Be(new FranchiseRecord(2, 1, 1, 0, 2, 4));
    }

    [Fact]
    public async Task GetDetail_IntraFranchiseGame_CountedOnBothSides()
    {
        await SeedAsync(db =>
        {
            db.Franchises.Add(Franchise(1));
            db.Tournaments.Add(Tournament(1));
            db.Teams.AddRange(Team(10, 1, 1), Team(11, 1, 1));
            db.Games.Add(Game(1, homeId: 10, awayId: 11, homeGoals: 1, awayGoals: 1));
        });

        var detail = await NewService().GetDetailBySlugAsync("franchise-1");

        detail!.AllTime.Should().Be(new FranchiseRecord(2, 0, 0, 2, 2, 2));
        detail.IntraFranchiseGames.Should().Be(1);
    }

    [Fact]
    public async Task SkaterLeaders_AggregateAcrossSeasons_SortPointsGoalsGp()
    {
        await SeedAsync(db =>
        {
            db.Franchises.Add(Franchise(1));
            db.Teams.AddRange(Team(10, 1, 1), Team(20, 1, 2));
            db.SkaterStats.AddRange(
                // Account 1: 2 seasons, 3 GP, 3 G + 1 A = 4 P. Latest row carries the new surname.
                Skater(1, accountId: 1, teamId: 10, tournamentId: 1, goals: 1, assists: 1, lastName: "Maiden"),
                Skater(2, accountId: 1, teamId: 10, tournamentId: 1, goals: 1, assists: 0, lastName: "Maiden"),
                Skater(3, accountId: 1, teamId: 20, tournamentId: 2, goals: 1, assists: 0, lastName: "Married"),
                // Account 2: 4 P but only 2 G -> below account 1 on goals.
                Skater(1, accountId: 2, teamId: 10, tournamentId: 1, goals: 2, assists: 2),
                // Account 3: 4 P, 3 G in 2 GP -> above account 1 on fewer games played.
                Skater(1, accountId: 3, teamId: 10, tournamentId: 1, goals: 2, assists: 1),
                Skater(2, accountId: 3, teamId: 10, tournamentId: 1, goals: 1, assists: 0),
                // Account 4: most points.
                Skater(3, accountId: 4, teamId: 20, tournamentId: 2, goals: 0, assists: 6));
        });

        var leaders = (await NewService().GetDetailBySlugAsync("franchise-1"))!.SkaterLeaders;

        leaders.Select(l => l.AccountId).Should().Equal(4, 3, 1, 2);
        var one = leaders.Single(l => l.AccountId == 1);
        one.Should().Be(new FranchiseSkaterLeader(1, 1, "First1", "Married", null, Seasons: 2, GamesPlayed: 3, Goals: 3, Assists: 1, Points: 4));
    }

    [Fact]
    public async Task SkaterLeaders_IgnoreOtherFranchisesRows()
    {
        await SeedAsync(db =>
        {
            db.Franchises.AddRange(Franchise(1), Franchise(2));
            db.Teams.AddRange(Team(10, 1, 1), Team(11, 2, 1));
            db.SkaterStats.AddRange(
                Skater(1, accountId: 1, teamId: 10, tournamentId: 1, goals: 1, assists: 0),
                Skater(2, accountId: 1, teamId: 11, tournamentId: 1, goals: 5, assists: 5),
                Skater(2, accountId: 2, teamId: 11, tournamentId: 1, goals: 9, assists: 0));
        });

        var leaders = (await NewService().GetDetailBySlugAsync("franchise-1"))!.SkaterLeaders;

        leaders.Should().ContainSingle();
        leaders[0].Should().Match<FranchiseSkaterLeader>(l => l.AccountId == 1 && l.Points == 1 && l.GamesPlayed == 1);
    }

    [Fact]
    public async Task GoalieLeaders_SortWinsThenGaa_RequireTwoGames()
    {
        await SeedAsync(db =>
        {
            db.Franchises.Add(Franchise(1));
            db.Teams.AddRange(Team(10, 1, 1), Team(20, 1, 2));
            db.GoalieStats.AddRange(
                // Account 1: 2 W, GAA 2.0
                Goalie(1, accountId: 1, teamId: 10, tournamentId: 1, wins: 1, goalsAgainst: 2, shotsAgainst: 20),
                Goalie(2, accountId: 1, teamId: 20, tournamentId: 2, wins: 1, goalsAgainst: 2, shotsAgainst: 20),
                // Account 2: 2 W, GAA 1.0 -> above account 1
                Goalie(1, accountId: 2, teamId: 10, tournamentId: 1, wins: 1, goalsAgainst: 1, shotsAgainst: 20),
                Goalie(2, accountId: 2, teamId: 10, tournamentId: 1, wins: 1, goalsAgainst: 1, shotsAgainst: 20),
                // Account 3: 1 W in 2 GP
                Goalie(3, accountId: 3, teamId: 20, tournamentId: 2, wins: 1, goalsAgainst: 0, shotsAgainst: 20),
                Goalie(4, accountId: 3, teamId: 20, tournamentId: 2, wins: 0, goalsAgainst: 5, shotsAgainst: 20),
                // Account 4: 1 GP only -> not eligible
                Goalie(5, accountId: 4, teamId: 20, tournamentId: 2, wins: 1, goalsAgainst: 0, shotsAgainst: 30));
        });

        var leaders = (await NewService().GetDetailBySlugAsync("franchise-1"))!.GoalieLeaders;

        leaders.Select(l => l.AccountId).Should().Equal(2, 1, 3);
        var one = leaders.Single(l => l.AccountId == 1);
        one.Seasons.Should().Be(2);
        one.GamesPlayed.Should().Be(2);
        one.Wins.Should().Be(2);
        one.GoalsAgainstAverage.Should().Be(2.0);
    }

    [Fact]
    public async Task GoalieLeaders_SavePercentageZeroWhenNoShots()
    {
        await SeedAsync(db =>
        {
            db.Franchises.Add(Franchise(1));
            db.Teams.Add(Team(10, 1, 1));
            db.GoalieStats.AddRange(
                Goalie(1, accountId: 1, teamId: 10, tournamentId: 1, wins: 0, goalsAgainst: 0, shotsAgainst: 0),
                Goalie(2, accountId: 1, teamId: 10, tournamentId: 1, wins: 0, goalsAgainst: 0, shotsAgainst: 0));
        });

        var leader = (await NewService().GetDetailBySlugAsync("franchise-1"))!.GoalieLeaders.Single();

        leader.SavePercentage.Should().Be(0);
        leader.GoalsAgainstAverage.Should().Be(0);
    }

    [Fact]
    public async Task GoalieLeaders_SavePercentageAndGaa_AreFractional()
    {
        await SeedAsync(db =>
        {
            db.Franchises.Add(Franchise(1));
            db.Teams.Add(Team(10, 1, 1));
            db.GoalieStats.AddRange(
                Goalie(1, accountId: 1, teamId: 10, tournamentId: 1, wins: 1, goalsAgainst: 2, shotsAgainst: 20),
                Goalie(2, accountId: 1, teamId: 10, tournamentId: 1, wins: 0, goalsAgainst: 1, shotsAgainst: 10));
        });

        var leader = (await NewService().GetDetailBySlugAsync("franchise-1"))!.GoalieLeaders.Single();

        // 27 saves / 30 shots and 3 goals against / 2 games; integer division would give 0 and 1.
        leader.SavePercentage.Should().BeApproximately(0.9, 1e-9);
        leader.GoalsAgainstAverage.Should().Be(1.5);
    }

    [Fact]
    public async Task Merge_MovesAllTeams_DedupesOwners_KeepsTargetBrand_DeletesSource()
    {
        await SeedAsync(db =>
        {
            db.Accounts.AddRange(Account(1), Account(2), Account(3));
            var source = Franchise(1, "Source");
            source.Owners.Add(db.Accounts.Local.Single(a => a.Id == 1));
            source.Owners.Add(db.Accounts.Local.Single(a => a.Id == 2));
            var target = Franchise(2, "Target");
            target.Owners.Add(db.Accounts.Local.Single(a => a.Id == 2));
            target.Owners.Add(db.Accounts.Local.Single(a => a.Id == 3));
            db.Franchises.AddRange(source, target);
            db.Teams.AddRange(Team(10, 1, 1), Team(20, 1, 2), Team(30, 2, 3));
        });

        await NewService().MergeAsync(1, 2);

        await using var db = await _factory.CreateDbContextAsync();
        (await db.Franchises.AnyAsync(f => f.Id == 1)).Should().BeFalse();
        (await db.Teams.Where(t => t.FranchiseId == 2).Select(t => t.Id).ToListAsync()).Should().BeEquivalentTo([10, 20, 30]);
        var owners = await db.FranchiseOwners.Where(o => o.FranchiseId == 2).Select(o => o.AccountId).ToListAsync();
        owners.Should().BeEquivalentTo([1, 2, 3]);
        (await db.FranchiseOwners.AnyAsync(o => o.FranchiseId == 1)).Should().BeFalse();
        var target = await db.Franchises.SingleAsync(f => f.Id == 2);
        target.Name.Should().Be("Target");
        target.Slug.Should().Be("franchise-2");
    }

    [Fact]
    public async Task Merge_AllowsOverlappingTournaments()
    {
        await SeedAsync(db =>
        {
            db.Franchises.AddRange(Franchise(1), Franchise(2));
            db.Teams.AddRange(Team(10, 1, 1), Team(11, 2, 1));
        });

        await NewService().MergeAsync(1, 2);

        await using var db = await _factory.CreateDbContextAsync();
        (await db.Teams.CountAsync(t => t.FranchiseId == 2 && t.TournamentId == 1)).Should().Be(2);
    }

    [Fact]
    public async Task Merge_SameIds_Throws_AndChangesNothing()
    {
        await SeedAsync(db =>
        {
            db.Franchises.Add(Franchise(1));
            db.Teams.Add(Team(10, 1, 1));
        });

        var act = () => NewService().MergeAsync(1, 1);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await using var db = await _factory.CreateDbContextAsync();
        (await db.Franchises.AnyAsync(f => f.Id == 1)).Should().BeTrue();
        (await db.Teams.SingleAsync()).FranchiseId.Should().Be(1);
    }

    [Fact]
    public async Task Merge_MissingTarget_Throws_AndChangesNothing()
    {
        await SeedAsync(db =>
        {
            db.Accounts.Add(Account(1));
            var source = Franchise(1);
            source.Owners.Add(db.Accounts.Local.Single());
            db.Franchises.Add(source);
            db.Teams.Add(Team(10, 1, 1));
        });

        var act = () => NewService().MergeAsync(1, 404);

        await act.Should().ThrowAsync<EntityNotFoundException>();
        await using var db = await _factory.CreateDbContextAsync();
        (await db.Franchises.AnyAsync(f => f.Id == 1)).Should().BeTrue();
        (await db.Teams.SingleAsync()).FranchiseId.Should().Be(1);
        (await db.FranchiseOwners.SingleAsync()).Should().Match<FranchiseOwner>(o => o.FranchiseId == 1 && o.AccountId == 1);
    }

    [Fact]
    public async Task MoveTeam_ChangesOnlyThatTeam_LeavesSourceFranchise()
    {
        await SeedAsync(db =>
        {
            db.Franchises.AddRange(Franchise(1), Franchise(2));
            db.Teams.AddRange(Team(10, 1, 1), Team(20, 1, 2), Team(30, 2, 3));
        });

        await NewService().MoveTeamAsync(10, 2);

        await using var db = await _factory.CreateDbContextAsync();
        var franchiseByTeam = await db.Teams.ToDictionaryAsync(t => t.Id, t => t.FranchiseId);
        franchiseByTeam.Should().BeEquivalentTo(new Dictionary<int, int> { [10] = 2, [20] = 1, [30] = 2 });
        (await db.Franchises.AnyAsync(f => f.Id == 1)).Should().BeTrue();
    }

    [Fact]
    public async Task MoveTeam_MissingTarget_Throws()
    {
        await SeedAsync(db =>
        {
            db.Franchises.Add(Franchise(1));
            db.Teams.Add(Team(10, 1, 1));
        });

        var act = () => NewService().MoveTeamAsync(10, 404);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Delete_WithTeams_Throws()
    {
        await SeedAsync(db =>
        {
            db.Franchises.Add(Franchise(1));
            db.Teams.Add(Team(10, 1, 1));
        });

        var act = () => NewService().DeleteAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await using var db = await _factory.CreateDbContextAsync();
        (await db.Franchises.AnyAsync(f => f.Id == 1)).Should().BeTrue();
    }

    [Fact]
    public async Task Delete_WithoutTeams_Removes()
    {
        await SeedAsync(db => db.Franchises.Add(Franchise(1)));

        await NewService().DeleteAsync(1);

        await using var db = await _factory.CreateDbContextAsync();
        (await db.Franchises.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task SetOwners_AddsAndRemoves()
    {
        await SeedAsync(db =>
        {
            db.Accounts.AddRange(Account(1), Account(2), Account(3));
            var franchise = Franchise(1);
            franchise.Owners.Add(db.Accounts.Local.Single(a => a.Id == 1));
            franchise.Owners.Add(db.Accounts.Local.Single(a => a.Id == 2));
            db.Franchises.Add(franchise);
        });

        await NewService().SetOwnersAsync(1, [2, 3]);

        await using var db = await _factory.CreateDbContextAsync();
        (await db.FranchiseOwners.Select(o => o.AccountId).ToListAsync()).Should().BeEquivalentTo([2, 3]);
    }

    [Fact]
    public async Task CreateAndUpdateBrand_PersistFields()
    {
        var service = NewService();

        var created = await service.CreateAsync(new("Name", "Short", "ABC", "#000000", "#FFFFFF", null));
        await service.UpdateBrandAsync(created.Id, new("New", "NewShort", "NEW", "#111111", "#222222", "#333333"));

        await using var db = await _factory.CreateDbContextAsync();
        var franchise = await db.Franchises.SingleAsync();
        franchise.Should().Match<Franchise>(f =>
            f.Id == created.Id
            && f.Name == "New"
            && f.NameShort == "NewShort"
            && f.Abbreviation == "NEW"
            && f.PrimaryColorHex == "#111111"
            && f.SecondaryColorHex == "#222222"
            && f.TertiaryColorHex == "#333333");
    }

    [Fact]
    public async Task GetDetailBySlug_FindsFranchiseBySlug()
    {
        await SeedAsync(db => db.Franchises.AddRange(Franchise(1), Franchise(2)));

        var detail = await NewService().GetDetailBySlugAsync("franchise-2");

        detail!.Franchise.Id.Should().Be(2);
    }

    [Fact]
    public async Task Create_AssignsSlugFromName()
    {
        var created = await NewService().CreateAsync(new("Bolton Bruins", "Bruins", "BOL", "#000000", "#FFFFFF", null));

        await using var db = await _factory.CreateDbContextAsync();
        created.Slug.Should().Be("bolton-bruins");
        (await db.Franchises.SingleAsync()).Slug.Should().Be("bolton-bruins");
    }

    [Fact]
    public async Task Create_SuffixesSlugTakenByAnotherFranchise()
    {
        await SeedAsync(db =>
        {
            var existing = Franchise(1, "Bolton Bruins");
            existing.Slug = "bolton-bruins";
            db.Franchises.Add(existing);
        });

        var created = await NewService().CreateAsync(new("Bolton Bruins!", "Bruins", "BOL", "#000000", "#FFFFFF", null));

        created.Slug.Should().Be("bolton-bruins-2");
    }

    [Fact]
    public async Task Create_NameWithoutLetters_FallsBackToFranchiseSlug()
    {
        var created = await NewService().CreateAsync(new("!!!", "!", "X", "#000000", "#FFFFFF", null));

        created.Slug.Should().Be("franchise");
    }

    [Fact]
    public async Task ReserveSlugs_KeepsFreeSlugs_AndSuffixesCollisions()
    {
        await SeedAsync(db => db.Franchises.Add(Franchise(1)));

        var reserved = await NewService().ReserveSlugsAsync(
        [
            new SlugCandidate(0, "brand-new"),
            new SlugCandidate(0, "franchise-1"),
            new SlugCandidate(0, "franchise-1"),
        ]);

        reserved.Should().Equal("brand-new", "franchise-1-2", "franchise-1-3");
    }

    [Fact]
    public async Task ReserveSlugs_LetsAFranchiseKeepItsOwnSlug()
    {
        await SeedAsync(db => db.Franchises.Add(Franchise(1)));

        var reserved = await NewService().ReserveSlugsAsync(
        [
            new SlugCandidate(1, "franchise-1"),
            new SlugCandidate(0, "franchise-1"),
        ]);

        reserved.Should().Equal("franchise-1", "franchise-1-2");
    }

    [Fact]
    public async Task ReserveSlugs_SkipsSuffixesAlreadyInUse()
    {
        await SeedAsync(db =>
        {
            var suffixed = Franchise(2);
            suffixed.Slug = "franchise-1-2";
            db.Franchises.AddRange(Franchise(1), suffixed);
        });

        var reserved = await NewService().ReserveSlugsAsync([new SlugCandidate(0, "franchise-1")]);

        reserved.Should().Equal("franchise-1-3");
    }

    [Fact]
    public async Task ReserveSlugs_EmptyCandidates_ReturnsEmpty()
    {
        (await NewService().ReserveSlugsAsync([])).Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateLogo_CallsGenerateFinalKeyForFranchise()
    {
        await SeedAsync(db => db.Franchises.Add(Franchise(7)));
        var keyGenerator = new Mock<IAssetKeyGenerator>();
        keyGenerator
            .Setup(k => k.GenerateFinalKey<Franchise>("7", "logo", ".png"))
            .Returns("media/franchise/7/logo/guid.png");

        await NewService(keyGenerator: keyGenerator.Object).UpdateLogoAsync(7, "temp/abc.png");

        keyGenerator.Verify(k => k.GenerateFinalKey<Franchise>("7", "logo", ".png"), Times.Once);
        await using var db = await _factory.CreateDbContextAsync();
        (await db.Franchises.SingleAsync()).Logo.Should().Be("media/franchise/7/logo/guid.png");
    }

    [Fact]
    public async Task Telemetry_Merge_EmitsIdsAndCountsOnly()
    {
        await SeedAsync(db =>
        {
            db.Accounts.Add(Account(1));
            var source = Franchise(1);
            source.Owners.Add(db.Accounts.Local.Single());
            db.Franchises.AddRange(source, Franchise(2));
            db.Teams.AddRange(Team(10, 1, 1), Team(20, 1, 2));
        });
        var telemetry = new Mock<ITelemetry>();
        object?[]? attributes = null;
        telemetry
            .Setup(t => t.TrackEvent("franchise.merged", It.IsAny<object?[]>()))
            .Callback<string, object?[]>((_, a) => attributes = a);

        await NewService(telemetry.Object).MergeAsync(1, 2);

        attributes.Should().NotBeNull();
        var keys = attributes!.Where((_, i) => i % 2 == 0).Cast<string>().ToList();
        keys.Should().OnlyContain(k => k.EndsWith(".id") || k.EndsWith(".count"));
        var pairs = keys.Select((k, i) => (k, attributes?[i * 2 + 1])).ToDictionary(p => p.k, p => p.Item2);
        pairs.Should().Contain(new KeyValuePair<string, object?>("franchise.source.id", 1));
        pairs.Should().Contain(new KeyValuePair<string, object?>("franchise.target.id", 2));
        pairs.Should().Contain(new KeyValuePair<string, object?>("team.count", 2));
        pairs.Should().Contain(new KeyValuePair<string, object?>("owner.count", 1));
    }

    [Fact]
    public async Task Telemetry_SetOwners_EmitsAddedAndRemovedOwnerIds()
    {
        await SeedAsync(db =>
        {
            db.Accounts.AddRange(Account(1), Account(2), Account(3));
            var franchise = Franchise(1);
            franchise.Owners.Add(db.Accounts.Local.Single(a => a.Id == 1));
            franchise.Owners.Add(db.Accounts.Local.Single(a => a.Id == 2));
            db.Franchises.Add(franchise);
        });
        var telemetry = new Mock<ITelemetry>();
        object?[]? attributes = null;
        telemetry
            .Setup(t => t.TrackEvent("franchise.owners_set", It.IsAny<object?[]>()))
            .Callback<string, object?[]>((_, a) => attributes = a);

        await NewService(telemetry.Object).SetOwnersAsync(1, [2, 3]);

        attributes.Should().NotBeNull();
        var keys = attributes!.Where((_, i) => i % 2 == 0).Cast<string>().ToList();
        keys.Should().OnlyContain(k => k.EndsWith(".id") || k.EndsWith(".ids") || k.EndsWith(".count"));
        var pairs = keys.Select((k, i) => (k, attributes?[i * 2 + 1])).ToDictionary(p => p.k, p => p.Item2);
        pairs["franchise.id"].Should().Be(1);
        pairs["owner.count"].Should().Be(2);
        pairs["owner.added.ids"].Should().BeEquivalentTo(new[] { 3 });
        pairs["owner.removed.ids"].Should().BeEquivalentTo(new[] { 1 });
    }

    [Fact]
    public void NewSeasonTeam_CopiesBrandFields_SetsFranchiseIdOnly()
    {
        var franchise = Franchise(5, "Brand");
        franchise.Logo = "logo-key";
        franchise.Banner = "banner-key";
        franchise.TertiaryColorHex = "#ABCDEF";

        var team = franchise.NewSeasonTeam(tournamentId: 3);

        team.Should().Match<Team>(t =>
            t.Id == 0
            && t.FranchiseId == 5
            && t.TournamentId == 3
            && t.Name == "Brand"
            && t.NameShort == "Brand"
            && t.Abbreviation == "F5"
            && t.Logo == "logo-key"
            && t.Banner == "banner-key"
            && t.PrimaryColorHex == "#000000"
            && t.SecondaryColorHex == "#FFFFFF"
            && t.TertiaryColorHex == "#ABCDEF");
        team.Franchise.Should().BeNull();

        team.Name = "Team Renamed";
        franchise.Name = "Franchise Renamed";
        franchise.Name.Should().Be("Franchise Renamed");
        team.Name.Should().Be("Team Renamed");
    }
}