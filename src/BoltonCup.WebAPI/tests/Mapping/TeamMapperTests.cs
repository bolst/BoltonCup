using BoltonCup.Core;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Mapping;

public class TeamMapperTests
{
    readonly Mock<IAssetUrlResolver> _urlResolver = new Mock<IAssetUrlResolver>();
    readonly Mapper _mapper;

    public TeamMapperTests()
    {
        _urlResolver.Setup(r => r.GetFullUrl(It.IsAny<string?>())).Returns((string? key) => key is null ? null : $"https://cdn/{key}");
        _mapper = new Mapper(_urlResolver.Object);
    }

    static Team NewTeam(Franchise? franchise) => new()
    {
        Id = 10,
        FranchiseId = 3,
        Franchise = franchise!,
        TournamentId = 1,
        Tournament = new Tournament { Id = 1, Name = "Bolton Cup" },
        Name = "Bears 2026",
        NameShort = "Bears",
        Abbreviation = "B26",
        Logo = "media/team/10/logo/a.png",
        Banner = "media/team/10/banner/b.png",
        PrimaryColorHex = "#000000",
        SecondaryColorHex = "#FFFFFF",
        TertiaryColorHex = "#888888",
    };

    static Franchise NewFranchise() => new()
    {
        Id = 3,
        Name = "Bolton Bears",
        Slug = "bolton-bears",
        NameShort = "Bears",
        Abbreviation = "BB",
        Logo = "media/franchise/3/logo/f.png",
        PrimaryColorHex = "#111111",
        SecondaryColorHex = "#222222",
    };

    static TeamBriefDto? BriefOf(Mapper mapper, Team team)
        => mapper.ToDto(new DraftPick { DraftId = 1, OverallPick = 1, Round = 1, RoundPick = 1, TeamId = team.Id, Team = team })!.Team;

    [Fact]
    public void ToTeamBriefDto_FranchiseNotLoaded_SetsIdOnly()
    {
        var brief = BriefOf(_mapper, NewTeam(franchise: null))!;

        brief.FranchiseId.Should().Be(3);
        brief.Franchise.Should().BeNull();
    }

    [Fact]
    public void ToTeamBriefDto_FranchiseLoaded_SetsBrief_AndKeepsExistingFields()
    {
        var brief = BriefOf(_mapper, NewTeam(NewFranchise()))!;

        brief.FranchiseId.Should().Be(3);
        brief.Franchise.Should().Be(new FranchiseBriefDto
        {
            Id = 3,
            Name = "Bolton Bears",
            Slug = "bolton-bears",
            NameShort = "Bears",
            Abbreviation = "BB",
            LogoUrl = "https://cdn/media/franchise/3/logo/f.png",
            PrimaryColorHex = "#111111",
            SecondaryColorHex = "#222222",
        });
        brief.Id.Should().Be(10);
        brief.Name.Should().Be("Bears 2026");
        brief.NameShort.Should().Be("Bears");
        brief.Abbreviation.Should().Be("B26");
        brief.Logo.Should().Be("https://cdn/media/team/10/logo/a.png");
        brief.Banner.Should().Be("https://cdn/media/team/10/banner/b.png");
        brief.PrimaryColorHex.Should().Be("#000000");
        brief.SecondaryColorHex.Should().Be("#FFFFFF");
        brief.TertiaryColorHex.Should().Be("#888888");
    }

    [Fact]
    public void ToTeamSingleDto_SetsFranchiseIdAndBrief()
    {
        var dto = _mapper.ToDto(NewTeam(NewFranchise()))!;

        dto.FranchiseId.Should().Be(3);
        dto.Franchise!.Name.Should().Be("Bolton Bears");
        dto.Name.Should().Be("Bears 2026");
    }

    static GameDto GameOf(Mapper mapper, Team home, Team away)
    {
        var game = new Game
        {
            Id = 1,
            TournamentId = 1,
            Tournament = new Tournament { Id = 1, Name = "Bolton Cup" },
            GameTime = new DateTime(2026, 8, 1),
            HomeTeamId = home.Id,
            HomeTeam = home,
            AwayTeamId = away.Id,
            AwayTeam = away,
        };
        var paged = new Mock<IPagedList<Game>>();
        paged.SetupGet(p => p.Items).Returns([game]);
        return mapper.ToDtoList(paged.Object).Items.Single();
    }

    [Fact]
    public void ToTeamInGameDto_SetsFranchiseIdAndBrief()
    {
        var away = NewTeam(franchise: null);
        away.Id = 11;
        away.FranchiseId = 4;

        var dto = GameOf(_mapper, NewTeam(NewFranchise()), away);

        dto.HomeTeam!.FranchiseId.Should().Be(3);
        dto.HomeTeam.Franchise!.Name.Should().Be("Bolton Bears");
        dto.AwayTeam!.FranchiseId.Should().Be(4);
        dto.AwayTeam.Franchise.Should().BeNull();
    }

    [Fact]
    public void ToTeamSingleDto_FranchiseNotLoaded_SetsIdOnly()
    {
        var dto = _mapper.ToDto(NewTeam(franchise: null))!;

        dto.FranchiseId.Should().Be(3);
        dto.Franchise.Should().BeNull();
    }
}