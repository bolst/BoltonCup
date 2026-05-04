using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface ITournamentMapper
{
    GetTournamentsQuery ToQuery(GetTournamentsRequest request);
    IPagedList<TournamentDto> ToDtoList(IPagedList<Tournament> tournaments);
    TournamentSingleDto? ToDto(Tournament? tournament);
    PlayerStatLeadersDto ToDto(string title, IEnumerable<SkaterStat> stats, Func<SkaterStat, double> selector, string? format = null);
    PlayerStatLeadersDto ToDto(string title, IEnumerable<GoalieStat> stats, Func<GoalieStat, double> selector, string? format = null);
}

public class TournamentMapper(IAssetUrlResolver _urlResolver, IBriefMapper _briefMapper) : ITournamentMapper
{
    public GetTournamentsQuery ToQuery(GetTournamentsRequest request)
    {
        return new GetTournamentsQuery
        {
            RegistrationOpen = request.RegistrationOpen,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }
    
    public IPagedList<TournamentDto> ToDtoList(IPagedList<Tournament> tournaments)
    {
        return tournaments.ProjectTo(tournament => new TournamentDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            Logo = _urlResolver.GetFullUrl(tournament.Logo),
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            WinningTeamId = tournament.WinningTeamId,
            IsActive = tournament.IsActive,
            IsRegistrationOpen = tournament.IsRegistrationOpen,
            IsPaymentOpen = tournament.IsPaymentOpen,
            SkaterLimit = tournament.SkaterLimit,
            GoalieLimit = tournament.GoalieLimit,
            Gallery = tournament.Gallery is null ? null : _briefMapper.ToGalleryBriefDto(tournament.Gallery)
        });
    }    
    
    
    public TournamentSingleDto? ToDto(Tournament? tournament)
    {
        return tournament is null
            ? null
            : new TournamentSingleDto 
            { 
                Id = tournament.Id,
                Name = tournament.Name,
                Logo = _urlResolver.GetFullUrl(tournament.Logo),
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                WinningTeamId = tournament.WinningTeamId,
                IsActive = tournament.IsActive,
                IsRegistrationOpen = tournament.IsRegistrationOpen,
                IsPaymentOpen = tournament.IsPaymentOpen,
                SkaterLimit = tournament.SkaterLimit,
                GoalieLimit = tournament.GoalieLimit,
                InfoGuide = tournament.InfoGuide is null ? null : _briefMapper.ToInfoGuideBriefDto(tournament.InfoGuide),
                Gallery = tournament.Gallery is null ? null : _briefMapper.ToGalleryBriefDto(tournament.Gallery),
                Sponsors = tournament.Sponsors
                    .Select(sponsor => new TournamentSponsorDto
                    {
                        Name = sponsor.Name,
                        LogoUrl = _urlResolver.GetFullUrl(sponsor.Logo),
                        WebsiteUrl = sponsor.WebsiteUrl,
                    })
                    .ToList()
            };
    }
    
    
    public PlayerStatLeadersDto ToDto(string title, IEnumerable<SkaterStat> stats, Func<SkaterStat, double> selector, string? format = null)
    {
        return new PlayerStatLeadersDto
        {
            Title = title,
            Leaders = stats.Select(stat => new PlayerStatDto
            {
                PlayerId = stat.PlayerId,
                AccountId = stat.AccountId,
                FirstName = stat.FirstName,
                LastName = stat.LastName,
                Position = stat.Position,
                JerseyNumber = stat.JerseyNumber,
                Birthday = stat.Birthday,
                ProfilePicture = _urlResolver.GetFullUrl(stat.ProfilePicture),
                TeamId = stat.TeamId,
                TeamName = stat.TeamName,
                TeamLogoUrl = _urlResolver.GetFullUrl(stat.TeamLogoUrl),
                TeamAbbreviation = stat.TeamAbbreviation,
                StatValue = selector(stat),
                StatString = selector(stat).ToString(format)
            })
        };
    }
    
    
    public PlayerStatLeadersDto ToDto(string title, IEnumerable<GoalieStat> stats, Func<GoalieStat, double> selector, string? format = null)
    {
        return new PlayerStatLeadersDto
        {
            Title = title,
            Leaders = stats.Select(stat => new PlayerStatDto
            {
                PlayerId = stat.PlayerId,
                AccountId = stat.AccountId,
                FirstName = stat.FirstName,
                LastName = stat.LastName,
                Position = stat.Position,
                JerseyNumber = stat.JerseyNumber,
                Birthday = stat.Birthday,
                ProfilePicture = _urlResolver.GetFullUrl(stat.ProfilePicture),
                TeamId = stat.TeamId,
                TeamName = stat.TeamName,
                TeamLogoUrl = _urlResolver.GetFullUrl(stat.TeamLogoUrl),
                TeamAbbreviation = stat.TeamAbbreviation,
                StatValue = selector(stat),
                StatString = selector(stat).ToString(format)
            })
        };
    }
}
