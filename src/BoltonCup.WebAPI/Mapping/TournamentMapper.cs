using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface ITournamentMapper
{
    GetTournamentsQuery ToQuery(GetTournamentsRequest request);
    IPagedList<TournamentDto> ToDtoList(IPagedList<Tournament> tournaments);
    TournamentSingleDto? ToDto(Tournament? tournament);
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
            };
    }
}
