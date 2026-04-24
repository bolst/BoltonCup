using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Extensions;

namespace BoltonCup.WebAPI.Mapping;

public interface IDraftMapper
{
    IPagedList<DraftDto> ToDtoList(IPagedList<Draft> drafts);
    IPagedList<DraftPickDto> ToDtoList(IPagedList<DraftPick> draftPicks);
    IPagedList<DraftRankingDto> ToDtoList(IPagedList<PlayerDraftRanking> rankings);
    DraftSingleDto? ToDto(Draft? draft);
    DraftPickSingleDto? ToDto(DraftPick? draftPick);
    GetDraftsQuery ToQuery(GetDraftsRequest request);
    CreateDraftCommand ToCommand(CreateDraftRequest request);
    UpdateDraftCommand ToCommand(int id, UpdateDraftRequest request);
    UpdateDraftOrderingCommand ToCommand(int id, UpdateDraftOrderingRequest request);
    DraftPlayerCommand ToCommand(int id, DraftPlayerRequest request);
}

public class DraftMapper(IBriefMapper _briefMapper) : IDraftMapper
{
    public IPagedList<DraftDto> ToDtoList(IPagedList<Draft> drafts)
    {
        return drafts.ProjectTo(draft => new DraftDto
        {
            Id = draft.Id,
            Title = draft.Title,
            Type = draft.Type,
            Status = draft.Status,
            Tournament = _briefMapper.ToTournamentBriefDto(draft.Tournament),
        });
    }

    public IPagedList<DraftPickDto> ToDtoList(IPagedList<DraftPick> draftPicks)
    {
        return draftPicks.ProjectTo(draft => new DraftPickDto
        {
            DraftId = draft.DraftId,
            OverallPick = draft.OverallPick,
            Team = _briefMapper.ToTeamBriefDto(draft.Team),
            Player = draft.Player is null ? null : _briefMapper.ToPlayerBriefDto(draft.Player),
        });
    }

    public IPagedList<DraftRankingDto> ToDtoList(IPagedList<PlayerDraftRanking> rankings)
    {
        return rankings.ProjectTo(draft => new DraftRankingDto
        {
            Id = draft.Id,
            DraftId = draft.DraftId,
            Player = _briefMapper.ToPlayerBriefDto(draft.Player),
            Tournament = _briefMapper.ToTournamentBriefDto(draft.Tournament),
            Team = draft.DraftPick is null ? null : _briefMapper.ToTeamBriefDto(draft.DraftPick.Team),
            OverallPick = draft.DraftPick?.OverallPick,
            GamesPlayed = draft.GamesPlayed,
            TotalPoints = draft.TotalPoints,
            IsChampion = draft.IsChampion,
            DraftRanking = draft.DraftRanking,
            OverrideRanking = draft.OverrideRanking,
            IsDrafted = draft.IsDrafted,
            PointsPerGame = draft.PointsPerGame,
        });
    }
    
    public DraftSingleDto? ToDto(Draft? draft)
    {
        if (draft is null)
            return null;
        return new DraftSingleDto
        {
            Id = draft.Id,
            Title = draft.Title,
            Type = draft.Type,
            Status = draft.Status,
            Tournament = _briefMapper.ToTournamentBriefDto(draft.Tournament),
            PickOrder = draft.DraftOrders.Select(order => new DraftPickOrderDto
            {
                Pick = order.Pick,
                Team = _briefMapper.ToTeamBriefDto(order.Team)
            }).OrderBy(d => d.Pick)
        };
    }

    public DraftPickSingleDto? ToDto(DraftPick? draftPick)
    {
        if (draftPick is null)
            return null;
        return new DraftPickSingleDto
        {
            DraftId = draftPick.DraftId,
            OverallPick = draftPick.OverallPick,
            Team = _briefMapper.ToTeamBriefDto(draftPick.Team),
            Player = draftPick.Player is null ? null : _briefMapper.ToPlayerBriefDto(draftPick.Player),
        };
    }

    public GetDraftsQuery ToQuery(GetDraftsRequest request)
    {
        return new GetDraftsQuery
        {
            TournamentId = request.TournamentId,
            Status = request.Status
        };
    }
    
    public CreateDraftCommand ToCommand(CreateDraftRequest request)
    {
        return new CreateDraftCommand(
            TournamentId: request.TournamentId,
            Title: request.Title
        );
    }
    
    public UpdateDraftCommand ToCommand(int id, UpdateDraftRequest request)
    {
        return new UpdateDraftCommand(
            DraftId: id,
            DraftType: request.DraftType,
            DraftStatus: request.DraftStatus,
            Title: request.Title
        );
    }

    public UpdateDraftOrderingCommand ToCommand(int id, UpdateDraftOrderingRequest request)
    {
        return new UpdateDraftOrderingCommand(
            DraftId: id,
            Ordering: request.Ordering.Select(x => new DraftOrderCommandEntry(x.TeamId, x.Pick)).ToList()
        );
    }
    
    public DraftPlayerCommand ToCommand(int id, DraftPlayerRequest request)
    {
        return new DraftPlayerCommand(
            DraftId: id,
            PlayerId: request.PlayerId,
            TeamId: request.TeamId,
            OverallPick: request.OverallPick
        );
    }
    
}