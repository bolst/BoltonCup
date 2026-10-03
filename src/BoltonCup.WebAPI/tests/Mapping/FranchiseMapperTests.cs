using BoltonCup.Core;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Mapping;

public class FranchiseMapperTests
{
    readonly Mock<IAssetUrlResolver> _urlResolver = new Mock<IAssetUrlResolver>();
    readonly Mapper _mapper;

    public FranchiseMapperTests()
    {
        _urlResolver.Setup(r => r.GetFullUrl(It.IsAny<string?>())).Returns((string? key) => key is null ? null : $"https://cdn/{key}");
        _mapper = new Mapper(_urlResolver.Object);
    }

    static Franchise NewFranchise() => new()
    {
        Id = 3,
        Name = "Bolton Bears",
        Slug = "bolton-bears",
        NameShort = "Bears",
        Abbreviation = "BB",
        Logo = "media/franchise/3/logo/a.png",
        Banner = "media/franchise/3/banner/b.png",
        PrimaryColorHex = "#111111",
        SecondaryColorHex = "#222222",
        TertiaryColorHex = "#333333",
    };

    static Account NewAccount(int id, string? avatar = null) => new()
    {
        Id = id,
        FirstName = $"First{id}",
        LastName = $"Last{id}",
        Email = $"a{id}@test.com",
        Birthday = new DateTime(1990, 1, 1),
        Avatar = avatar,
    };

    [Fact]
    public void ToDtoList_MapsBrandAndCounts_InOrder()
    {
        var other = NewFranchise();
        other.Id = 4;
        other.Logo = null;
        IReadOnlyList<FranchiseSummary> summaries = [new(NewFranchise(), 2, 5), new(other, 0, 1)];

        var dtos = _mapper.ToDtoList(summaries);

        dtos.Select(d => d.Id).Should().Equal(3, 4);
        var first = dtos[0];
        first.Name.Should().Be("Bolton Bears");
        first.Slug.Should().Be("bolton-bears");
        first.NameShort.Should().Be("Bears");
        first.Abbreviation.Should().Be("BB");
        first.LogoUrl.Should().Be("https://cdn/media/franchise/3/logo/a.png");
        first.PrimaryColorHex.Should().Be("#111111");
        first.SecondaryColorHex.Should().Be("#222222");
        first.TertiaryColorHex.Should().Be("#333333");
        first.TitleCount.Should().Be(2);
        first.SeasonCount.Should().Be(5);
        dtos[1].LogoUrl.Should().BeNull();
    }

    [Fact]
    public void ToDto_Null_ReturnsNull()
    {
        _mapper.ToDto((FranchiseDetail?)null).Should().BeNull();
    }

    [Fact]
    public void ToDto_MapsRecordSeasonsTitlesOwnersAndLeaders()
    {
        var tournament = new Tournament { Id = 9, Name = "Bolton Cup 2025", WinningTeamId = 30 };
        var team = new Team
        {
            Id = 30,
            FranchiseId = 3,
            TournamentId = 9,
            Name = "Bears 2025",
            NameShort = "Bears",
            Abbreviation = "B25",
            Logo = "media/team/30/logo/c.png",
            PrimaryColorHex = "#000000",
            SecondaryColorHex = "#FFFFFF",
        };
        var detail = new FranchiseDetail
        {
            Franchise = NewFranchise(),
            Owners = [NewAccount(1, "avatars/1.png")],
            Titles = [tournament],
            AllTime = new FranchiseRecord(10, 6, 3, 1, 40, 25),
            IntraFranchiseGames = 1,
            Seasons = [new FranchiseSeason(team, tournament, new FranchiseRecord(5, 4, 1, 0, 20, 10), [NewAccount(2)], IsChampion: true)],
            SkaterLeaders = [new FranchiseSkaterLeader(11, 111, "Sk", "Ater", "avatars/11.png", 2, 8, 5, 6, 11)],
            GoalieLeaders = [new FranchiseGoalieLeader(12, 112, "Go", "Alie", null, 1, 4, 3, 1, 90, 100, 0.9, 2.5)],
        };

        var dto = _mapper.ToDto(detail)!;

        dto.Id.Should().Be(3);
        dto.Slug.Should().Be("bolton-bears");
        dto.LogoUrl.Should().Be("https://cdn/media/franchise/3/logo/a.png");
        dto.BannerUrl.Should().Be("https://cdn/media/franchise/3/banner/b.png");
        dto.IntraFranchiseGames.Should().Be(1);
        dto.AllTime.Should().Be(new FranchiseRecordDto { GamesPlayed = 10, Wins = 6, Losses = 3, Ties = 1, GoalsFor = 40, GoalsAgainst = 25 });

        dto.Owners.Should().ContainSingle().Which.Should().Be(new FranchiseOwnerDto
        {
            AccountId = 1,
            FirstName = "First1",
            LastName = "Last1",
            ProfilePictureUrl = "https://cdn/avatars/1.png",
        });
        dto.Titles.Should().ContainSingle().Which.Id.Should().Be(9);

        var season = dto.Seasons.Should().ContainSingle().Subject;
        season.TeamId.Should().Be(30);
        season.Name.Should().Be("Bears 2025");
        season.LogoUrl.Should().Be("https://cdn/media/team/30/logo/c.png");
        season.Tournament!.Id.Should().Be(9);
        season.Record.Wins.Should().Be(4);
        season.GeneralManagers.Should().ContainSingle().Which.AccountId.Should().Be(2);
        season.IsChampion.Should().BeTrue();

        var skater = dto.SkaterLeaders.Should().ContainSingle().Subject;
        skater.Should().Be(new FranchiseSkaterLeaderDto
        {
            AccountId = 11,
            PlayerId = 111,
            FirstName = "Sk",
            LastName = "Ater",
            ProfilePictureUrl = "https://cdn/avatars/11.png",
            Seasons = 2,
            GamesPlayed = 8,
            Goals = 5,
            Assists = 6,
            Points = 11,
        });

        var goalie = dto.GoalieLeaders.Should().ContainSingle().Subject;
        goalie.ProfilePictureUrl.Should().BeNull();
        goalie.Wins.Should().Be(3);
        goalie.Shutouts.Should().Be(1);
        goalie.Saves.Should().Be(90);
        goalie.ShotsAgainst.Should().Be(100);
        goalie.SavePercentage.Should().Be(0.9);
        goalie.GoalsAgainstAverage.Should().Be(2.5);
    }

    [Fact]
    public void ToDto_SeasonWithoutTournament_HasNullTournament()
    {
        var team = new Team
        {
            Id = 31,
            FranchiseId = 3,
            Name = "Bears",
            NameShort = "Bears",
            Abbreviation = "B",
            PrimaryColorHex = "#000000",
            SecondaryColorHex = "#FFFFFF",
        };
        var detail = new FranchiseDetail
        {
            Franchise = NewFranchise(),
            Owners = [],
            Titles = [],
            AllTime = FranchiseRecord.Empty,
            IntraFranchiseGames = 0,
            Seasons = [new FranchiseSeason(team, null, FranchiseRecord.Empty, [], IsChampion: false)],
            SkaterLeaders = [],
            GoalieLeaders = [],
        };

        _mapper.ToDto(detail)!.Seasons.Single().Tournament.Should().BeNull();
    }

    [Theory]
    [InlineData(typeof(FranchiseSkaterLeaderDto))]
    [InlineData(typeof(FranchiseGoalieLeaderDto))]
    public void LeaderDtos_HaveNoTeamFields(Type leaderDtoType)
    {
        leaderDtoType.GetProperties().Select(p => p.Name).Should().NotContain(name => name.Contains("Team"));
    }
}