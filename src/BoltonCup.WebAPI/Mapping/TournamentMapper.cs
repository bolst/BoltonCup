using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps <see cref="Tournament"/> entities to DTOs and queries.</summary>
public interface ITournamentMapper
{
    /// <summary>Maps a <see cref="GetTournamentsRequest"/> to a <see cref="GetTournamentsQuery"/>.</summary>
    GetTournamentsQuery ToQuery(GetTournamentsRequest request);

    /// <summary>Maps a paged list of <see cref="Tournament"/> entities to a paged list of <see cref="TournamentDto"/>.</summary>
    IPagedList<TournamentDto> ToDtoList(IPagedList<Tournament> tournaments);

    /// <summary>Maps a <see cref="Tournament"/> to a <see cref="TournamentSingleDto"/>, or returns <see langword="null"/> if the tournament is null.</summary>
    TournamentSingleDto? ToDto(Tournament? tournament);

    /// <summary>Maps a collection of skater stats to a player stat leaders DTO.</summary>
    PlayerStatLeadersDto ToDto(string title, IEnumerable<SkaterStat> stats, Func<SkaterStat, double> selector, string? format = null);

    /// <summary>Maps a collection of goalie stats to a player stat leaders DTO.</summary>
    PlayerStatLeadersDto ToDto(string title, IEnumerable<GoalieStat> stats, Func<GoalieStat, double> selector, string? format = null);
}

/// <summary>Maps <see cref="Tournament"/> entities to DTOs and queries.</summary>
public class TournamentMapper(IAssetUrlResolver _urlResolver, IBriefMapper _briefMapper) : ITournamentMapper
{
    /// <inheritdoc/>
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
    
    /// <inheritdoc/>
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
    
    
    /// <inheritdoc/>
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
    
    
    /// <inheritdoc/>
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
    
    
    /// <inheritdoc/>
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
