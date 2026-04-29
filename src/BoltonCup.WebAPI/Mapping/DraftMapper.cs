using BoltonCup.Core;
using BoltonCup.Core.Commands;

namespace BoltonCup.WebAPI.Mapping;

public interface IDraftMapper
{
    IPagedList<DraftDto> ToDtoList(IPagedList<Draft> drafts);
    IPagedList<DraftPickDto> ToDtoList(IPagedList<DraftPick> draftPicks);
    IPagedList<DraftRankingDto> ToDtoList(IPagedList<PlayerDraftRanking> rankings);
    DraftSingleDto? ToDto(Draft? draft);
    DraftPickSingleDto? ToDto(DraftPick? draftPick);
    DraftUpdateEventDto ToDto(CurrentDraftState draftState);
    DraftPickMadeEventDto ToDto(CurrentDraftStateWithPick draftState);
    GetDraftsQuery ToQuery(GetDraftsRequest request);
    CreateDraftCommand ToCommand(CreateDraftRequest request);
    UpdateDraftCommand ToCommand(UpdateDraftRequest request);
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
        return draftPicks.ProjectTo(ToDtoListItem);
    }

    public IPagedList<DraftRankingDto> ToDtoList(IPagedList<PlayerDraftRanking> rankings)
    {
        return rankings.ProjectTo(draft => new DraftRankingDto
        {
            Id = draft.Id,
            DraftId = draft.DraftId,
            TournamentId = draft.TournamentId,
            PlayerPhone = draft.Player.Account.Phone,
            Player = _briefMapper.ToPlayerBriefDto(draft.Player),
            DraftPick = _briefMapper.ToDraftPickBriefDto(draft.DraftPick),
            GamesPlayed = draft.GamesPlayed,
            TotalPoints = draft.TotalPoints,
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
            PickOrder = draft.DraftOrders
                .Select(order => new DraftPickOrderDto
                {
                    Pick = order.Pick,
                    Team = _briefMapper.ToTeamBriefDto(order.Team)
                })
                .OrderBy(d => d.Pick),
            DraftPicksByRound = draft.DraftPicks
                .GroupBy(dto => dto.Round)
                .Select(group => new RoundDraftPicks(group.Key, group.Select(ToDtoListItem).OrderBy(x => x.RoundPick)))
                .OrderBy(group => group.Round)
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
            Round = draftPick.Round,
            RoundPick = draftPick.RoundPick,
            Team = _briefMapper.ToTeamBriefDto(draftPick.Team),
            Player = draftPick.Player is null ? null : _briefMapper.ToPlayerBriefDto(draftPick.Player),
        };
    }

    public DraftUpdateEventDto ToDto(CurrentDraftState draftState)
    {
        return new DraftUpdateEventDto(
            Draft: ToDto(draftState.Draft)!,
            NextPick: ToDto(draftState.NextPick)
        );
    }

    public DraftPickMadeEventDto ToDto(CurrentDraftStateWithPick draftState)
    {
        return new DraftPickMadeEventDto(
            DraftId: draftState.Draft.Id,
            CompletedPick: _briefMapper.ToDraftPickBriefDto(draftState.CompletedPick)!,
            DraftedPlayer: _briefMapper.ToPlayerBriefDto(draftState.CompletedPick!.Player!),
            NextPick: ToDto(draftState.NextPick)
        );
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
    
    public UpdateDraftCommand ToCommand(UpdateDraftRequest request)
    {
        return new UpdateDraftCommand
        {
            Title = request.Title,
            DraftType = request.DraftType,
            Ordering = request.Ordering?
                .Select(x => new DraftOrderCommandEntry(x.TeamId, x.Pick))
                .ToList(),
        };
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


    public DraftPickDto ToDtoListItem(DraftPick draftPick)
    {
        return new DraftPickDto
        {
            DraftId = draftPick.DraftId,
            OverallPick = draftPick.OverallPick,
            Round = draftPick.Round,
            RoundPick = draftPick.RoundPick,
            Team = _briefMapper.ToTeamBriefDto(draftPick.Team),
            Player = draftPick.Player is null ? null : _briefMapper.ToPlayerBriefDto(draftPick.Player),
        };
    }
    
}