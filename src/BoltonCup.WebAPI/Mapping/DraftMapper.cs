using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Extensions;

namespace BoltonCup.WebAPI.Mapping;

public interface IDraftMapper
{
    IPagedList<DraftDto> ToDtoList(IPagedList<Draft> drafts);
    IPagedList<DraftPickDto> ToDtoList(IPagedList<DraftPick> draftPicks);
    DraftSingleDto? ToDto(Draft? draft);
    DraftPickSingleDto? ToDto(DraftPick? draftPick);
    GetDraftsQuery ToQuery(GetDraftsRequest request);
    CreateDraftCommand ToCommand(CreateDraftRequest request);
    UpdateDraftCommand ToCommand(int id, UpdateDraftRequest request);
    DraftPlayerCommand ToCommand(int id, DraftPlayerRequest request);
}

public class DraftMapper(IBriefMapper _briefMapper) : IDraftMapper
{
    public IPagedList<DraftDto> ToDtoList(IPagedList<Draft> drafts)
    {
        return drafts.ProjectTo(draft => new DraftDto
        {
            Id = draft.Id,
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
    
    public DraftSingleDto? ToDto(Draft? draft)
    {
        if (draft is null)
            return null;
        return new DraftSingleDto
        {
            Id = draft.Id,
            Type = draft.Type,
            Status = draft.Status,
            Tournament = _briefMapper.ToTournamentBriefDto(draft.Tournament),
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
            TournamentId: request.TournamentId
        );
    }
    
    public UpdateDraftCommand ToCommand(int id, UpdateDraftRequest request)
    {
        return new UpdateDraftCommand(
            DraftId: id,
            DraftType: request.DraftType
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