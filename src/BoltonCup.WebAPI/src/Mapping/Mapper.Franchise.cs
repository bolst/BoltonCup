using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

#pragma warning disable CS1591 // Disable warning for missing XML comments

public partial class Mapper
{
    // ---------- Franchise ----------

    public IReadOnlyList<FranchiseDto> ToDtoList(IReadOnlyList<FranchiseSummary> franchises) => franchises
        .Select(summary => new FranchiseDto
        {
            Id = summary.Franchise.Id,
            Name = summary.Franchise.Name,
            Slug = summary.Franchise.Slug,
            NameShort = summary.Franchise.NameShort,
            Abbreviation = summary.Franchise.Abbreviation,
            LogoUrl = _urlResolver.GetFullUrl(summary.Franchise.Logo),
            PrimaryColorHex = summary.Franchise.PrimaryColorHex,
            SecondaryColorHex = summary.Franchise.SecondaryColorHex,
            TertiaryColorHex = summary.Franchise.TertiaryColorHex,
            TitleCount = summary.TitleCount,
            SeasonCount = summary.SeasonCount,
        })
        .ToList();

    public FranchiseSingleDto? ToDto(FranchiseDetail? franchise) => franchise is null
            ? null
            : new FranchiseSingleDto
            {
                Id = franchise.Franchise.Id,
                Name = franchise.Franchise.Name,
                Slug = franchise.Franchise.Slug,
                NameShort = franchise.Franchise.NameShort,
                Abbreviation = franchise.Franchise.Abbreviation,
                LogoUrl = _urlResolver.GetFullUrl(franchise.Franchise.Logo),
                BannerUrl = _urlResolver.GetFullUrl(franchise.Franchise.Banner),
                PrimaryColorHex = franchise.Franchise.PrimaryColorHex,
                SecondaryColorHex = franchise.Franchise.SecondaryColorHex,
                TertiaryColorHex = franchise.Franchise.TertiaryColorHex,
                Owners = franchise.Owners
                    .Select(a => new FranchiseOwnerDto
                    {
                        AccountId = a.Id,
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        ProfilePictureUrl = _urlResolver.GetFullUrl(a.Avatar),
                    })
                    .ToList(),
                Titles = franchise.Titles
                    .Select(ToTournamentBriefDto)
                    .ToList(),
                AllTime = ToFranchiseRecordDto(franchise.AllTime),
                IntraFranchiseGames = franchise.IntraFranchiseGames,
                Seasons = franchise.Seasons
                    .Select(ToFranchiseSeasonDto)
                    .ToList(),
                SkaterLeaders = franchise.SkaterLeaders
                    .Select(l => new FranchiseSkaterLeaderDto
                    {
                        AccountId = l.AccountId,
                        PlayerId = l.PlayerId,
                        FirstName = l.FirstName,
                        LastName = l.LastName,
                        ProfilePictureUrl = _urlResolver.GetFullUrl(l.ProfilePicture),
                        Seasons = l.Seasons,
                        GamesPlayed = l.GamesPlayed,
                        Goals = l.Goals,
                        Assists = l.Assists,
                        Points = l.Points,
                    })
                    .ToList(),
                GoalieLeaders = franchise.GoalieLeaders
                    .Select(l => new FranchiseGoalieLeaderDto
                    {
                        AccountId = l.AccountId,
                        PlayerId = l.PlayerId,
                        FirstName = l.FirstName,
                        LastName = l.LastName,
                        ProfilePictureUrl = _urlResolver.GetFullUrl(l.ProfilePicture),
                        Seasons = l.Seasons,
                        GamesPlayed = l.GamesPlayed,
                        Wins = l.Wins,
                        Shutouts = l.Shutouts,
                        Saves = l.Saves,
                        ShotsAgainst = l.ShotsAgainst,
                        SavePercentage = l.SavePercentage,
                        GoalsAgainstAverage = l.GoalsAgainstAverage,
                    })
                    .ToList(),
            };

    FranchiseBriefDto? ToFranchiseBriefDtoOrNull(Franchise? franchise) => franchise is null
            ? null
            : new FranchiseBriefDto
            {
                Id = franchise.Id,
                Name = franchise.Name,
                Slug = franchise.Slug,
                NameShort = franchise.NameShort,
                Abbreviation = franchise.Abbreviation,
                LogoUrl = _urlResolver.GetFullUrl(franchise.Logo),
                PrimaryColorHex = franchise.PrimaryColorHex,
                SecondaryColorHex = franchise.SecondaryColorHex,
                TertiaryColorHex = franchise.TertiaryColorHex,
            };

    static FranchiseRecordDto ToFranchiseRecordDto(FranchiseRecord record) => new FranchiseRecordDto
    {
        GamesPlayed = record.GamesPlayed,
        Wins = record.Wins,
        Losses = record.Losses,
        Ties = record.Ties,
        GoalsFor = record.GoalsFor,
        GoalsAgainst = record.GoalsAgainst,
    };

    FranchiseSeasonDto ToFranchiseSeasonDto(FranchiseSeason season) => new FranchiseSeasonDto
    {
        TeamId = season.Team.Id,
        Name = season.Team.Name,
        LogoUrl = _urlResolver.GetFullUrl(season.Team.Logo),
        Tournament = season.Tournament is null ? null : ToTournamentBriefDto(season.Tournament),
        Record = ToFranchiseRecordDto(season.Record),
        GeneralManagers = season.GeneralManagers
            .Select(a => new TeamGmDto
            {
                AccountId = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                ProfilePictureUrl = _urlResolver.GetFullUrl(a.Avatar),
            })
            .ToList(),
        IsChampion = season.IsChampion,
    };
}