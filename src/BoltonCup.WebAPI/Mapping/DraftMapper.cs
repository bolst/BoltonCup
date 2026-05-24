using BoltonCup.Core;
using BoltonCup.Core.Commands;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps draft domain models to DTOs and commands.</summary>
public interface IDraftMapper
{
    /// <summary>Maps a paged list of drafts to a paged list of <see cref="DraftDto"/>.</summary>
    IPagedList<DraftDto> ToDtoList(IPagedList<Draft> drafts);
    /// <summary>Maps a paged list of draft picks to a paged list of <see cref="DraftPickDto"/>.</summary>
    IPagedList<DraftPickDto> ToDtoList(IPagedList<DraftPick> draftPicks);
    /// <summary>Maps a paged list of player draft rankings to a paged list of <see cref="DraftRankingDto"/>.</summary>
    IPagedList<DraftRankingDto> ToDtoList(IPagedList<PlayerDraftRanking> rankings);
    /// <summary>Maps a <see cref="Draft"/> to a <see cref="DraftSingleDto"/>.</summary>
    DraftSingleDto? ToDto(Draft? draft, bool isAuthorized);
    /// <summary>Maps a <see cref="DraftPick"/> to a <see cref="DraftPickSingleDto"/>.</summary>
    DraftPickSingleDto? ToDto(DraftPick? draftPick);
    /// <summary>Maps a <see cref="CurrentDraftState"/> to a <see cref="DraftUpdateEventDto"/>.</summary>
    DraftUpdateEventDto ToDto(CurrentDraftState draftState, bool isAuthorized);
    /// <summary>Maps a <see cref="CurrentDraftStateWithPick"/> to a <see cref="DraftPickMadeEventDto"/>.</summary>
    DraftPickMadeEventDto ToDto(CurrentDraftStateWithPick draftState);
    /// <summary>Maps a <see cref="GetDraftsRequest"/> to a <see cref="GetDraftsQuery"/>.</summary>
    GetDraftsQuery ToQuery(GetDraftsRequest request);
    /// <summary>Maps a <see cref="CreateDraftRequest"/> to a <see cref="CreateDraftCommand"/>.</summary>
    CreateDraftCommand ToCommand(CreateDraftRequest request);
    /// <summary>Maps an <see cref="UpdateDraftRequest"/> to an <see cref="UpdateDraftCommand"/>.</summary>
    UpdateDraftCommand ToCommand(UpdateDraftRequest request);
    /// <summary>Maps a draft player request to a <see cref="DraftPlayerCommand"/>.</summary>
    DraftPlayerCommand ToCommand(int id, DraftPlayerRequest request);
}

/// <summary>Maps draft domain models to DTOs and commands.</summary>
public class DraftMapper(IBriefMapper _briefMapper) : IDraftMapper
{
    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public IPagedList<DraftPickDto> ToDtoList(IPagedList<DraftPick> draftPicks)
    {
        return draftPicks.ProjectTo(ToDtoListItem);
    }

    /// <inheritdoc/>
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
    
    /// <inheritdoc/>
    public DraftSingleDto? ToDto(Draft? draft, bool isAuthorized)
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
                .OrderBy(group => group.Round),
            CanEditDraft = isAuthorized && draft.Status != DraftStatus.Completed,
        };
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public DraftUpdateEventDto ToDto(CurrentDraftState draftState, bool isAuthorized)
    {
        return new DraftUpdateEventDto(
            Draft: ToDto(draftState.Draft, isAuthorized)!,
            NextPick: ToDto(draftState.NextPick)
        );
    }

    /// <inheritdoc/>
    public DraftPickMadeEventDto ToDto(CurrentDraftStateWithPick draftState)
    {
        return new DraftPickMadeEventDto(
            DraftId: draftState.Draft.Id,
            CompletedPick: _briefMapper.ToDraftPickBriefDto(draftState.CompletedPick)!,
            DraftedPlayer: _briefMapper.ToPlayerBriefDto(draftState.CompletedPick!.Player!),
            NextPick: ToDto(draftState.NextPick)
        );
    }

    /// <inheritdoc/>
    public GetDraftsQuery ToQuery(GetDraftsRequest request)
    {
        return new GetDraftsQuery
        {
            TournamentId = request.TournamentId,
            Status = request.Status
        };
    }
    
    /// <inheritdoc/>
    public CreateDraftCommand ToCommand(CreateDraftRequest request)
    {
        return new CreateDraftCommand(
            TournamentId: request.TournamentId,
            Title: request.Title
        );
    }
    
    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public DraftPlayerCommand ToCommand(int id, DraftPlayerRequest request)
    {
        return new DraftPlayerCommand(
            DraftId: id,
            PlayerId: request.PlayerId,
            TeamId: request.TeamId,
            OverallPick: request.OverallPick
        );
    }


    /// <summary>Maps a <see cref="DraftPick"/> to a <see cref="DraftPickDto"/>.</summary>
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