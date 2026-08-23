using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- Draft ----------

    public IPagedList<DraftDto> ToDtoList(IPagedList<Draft> drafts) => drafts.ProjectTo(draft => new DraftDto
    {
        Id = draft.Id,
        Title = draft.Title,
        Type = draft.Type,
        Status = draft.Status,
        Tournament = ToTournamentBriefDto(draft.Tournament),
        CreatedByName = AccountName(draft.DraftOwner),
        IsVisible = draft.IsVisible,
        Rounds = draft.Rounds,
        Teams = draft.Teams,
        SecondsPerPick = draft.SecondsPerPick,
    });

    public IPagedList<DraftRankingDto> ToDtoList(IPagedList<PlayerDraftRanking> rankings, IReadOnlySet<int> favouritePlayerIds, TournamentAvailability availability) => rankings.ProjectTo(draft => new DraftRankingDto
    {
        Id = draft.Id,
        DraftId = draft.DraftId,
        TournamentId = draft.TournamentId,
        PlayerPhone = draft.Player.Account.Phone,
        Player = ToPlayerBriefDto(draft.Player),
        DraftPick = ToDraftPickBriefDto(draft.DraftPick),
        GamesPlayed = draft.GamesPlayed,
        TotalPoints = draft.TotalPoints,
        DraftRanking = draft.DraftRanking,
        OverrideRanking = draft.OverrideRanking,
        IsDrafted = draft.IsDrafted,
        PointsPerGame = draft.PointsPerGame,
        IsFavourite = favouritePlayerIds.Contains(draft.PlayerId),
        IsExcluded = draft.IsExcluded,
        GameAvailabilities = BuildAvailability(availability, draft.Player.AccountId),
    });

    public IReadOnlyList<PlayerAvailabilityDto> ToPlayerAvailabilityList(TournamentAvailability availability)
        => availability.ByAccount.Keys
            .Select(accountId => new PlayerAvailabilityDto
            {
                AccountId = accountId,
                GameAvailabilities = BuildAvailability(availability, accountId),
            })
            .ToList();

    public DraftSingleDto? ToDto(Draft? draft, bool isAuthorized, bool canManage)
    {
        if (draft is null)
        {
            return null;
        }

        return new DraftSingleDto
        {
            Id = draft.Id,
            Title = draft.Title,
            Type = draft.Type,
            Status = draft.Status,
            IsVisible = draft.IsVisible,
            Rounds = draft.Rounds,
            Teams = draft.Teams,
            SecondsPerPick = draft.SecondsPerPick,
            Tournament = ToTournamentBriefDto(draft.Tournament),
            CreatedByName = AccountName(draft.DraftOwner),
            PickOrder = draft.DraftOrders
                .Select(order => new DraftPickOrderDto
                {
                    Pick = order.Pick,
                    Team = ToTeamBriefDto(order.Team),
                    AutoPick = order.AutoPick
                })
                .OrderBy(d => d.Pick),
            DraftPicksByRound = draft.DraftPicks
                .GroupBy(dto => dto.Round)
                .Select(group => new RoundDraftPicks(
                    group.Key,
                    group.Select(dp => new DraftPickDto
                    {
                        DraftId = dp.DraftId,
                        OverallPick = dp.OverallPick,
                        Round = dp.Round,
                        RoundPick = dp.RoundPick,
                        Team = ToTeamBriefDto(dp.Team),
                        Player = dp.Player is null ? null : ToPlayerBriefDto(dp.Player),
                        ClockStartedAt = dp.ClockStartedAt,
                    }).ToList()))
                .OrderBy(group => group.Round),
            CanEditDraft = isAuthorized && draft.Status != DraftStatus.Completed,
            CanManageDraft = canManage,
            DefaultCustomRankingId = draft.DefaultCustomRankingId,
        };
    }

    public DraftPickSingleDto? ToDto(DraftPick? draftPick)
    {
        if (draftPick is null)
        {
            return null;
        }

        return new DraftPickSingleDto
        {
            DraftId = draftPick.DraftId,
            OverallPick = draftPick.OverallPick,
            Round = draftPick.Round,
            RoundPick = draftPick.RoundPick,
            Team = ToTeamBriefDto(draftPick.Team),
            Player = draftPick.Player is null ? null : ToPlayerBriefDto(draftPick.Player),
            ClockStartedAt = draftPick.ClockStartedAt,
        };
    }

    public DraftUpdateEventDto ToDto(CurrentDraftState draftState, bool isAuthorized, bool canManage) => new DraftUpdateEventDto(
            Draft: ToDto(draftState.Draft, isAuthorized, canManage)!,
            NextPick: ToDto(draftState.NextPick)
        );

    public DraftPickMadeEventDto ToDto(CurrentDraftStateWithPick draftState) => new DraftPickMadeEventDto(
            DraftId: draftState.Draft.Id,
            CompletedPick: ToDraftPickBriefDto(draftState.CompletedPick)!,
            DraftedPlayer: ToPlayerBriefDto(draftState.CompletedPick!.Player!),
            NextPick: ToDto(draftState.NextPick)
        );

    public GetDraftsQuery ToQuery(GetDraftsRequest request, ClaimsPrincipal user) => new GetDraftsQuery
    {
        TournamentId = request.TournamentId,
        Status = request.Status,
        AccountId = user.GetAccountIdOrDefault(),
        IsAdmin = user.IsInRole(BoltonCupRole.Admin),
    };

    public CreateDraftCommand ToCommand(CreateDraftRequest request, ClaimsPrincipal user) => new CreateDraftCommand(
            TournamentId: request.TournamentId,
            Title: request.Title,
            OwnerAccountId: user.GetAccountIdOrDefault()
        );

    public UpdateDraftCommand ToCommand(UpdateDraftRequest request) => new UpdateDraftCommand
    {
        Title = request.Title,
        DraftType = request.DraftType,
        Ordering = request.Ordering?
                .Select(x => new DraftOrderCommandEntry(x.TeamId, x.Pick))
                .ToList(),
        IsVisible = request.IsVisible,
        SecondsPerPick = request.SecondsPerPick,
        AutoPickSettings = request.AutoPickSettings?
                .Select(x => new DraftAutoPickEntry(x.TeamId, x.AutoPick))
                .ToList(),
    };

    public DraftPlayerCommand ToCommand(int id, DraftPlayerRequest request) => new DraftPlayerCommand(
            DraftId: id,
            PlayerId: request.PlayerId,
            TeamId: request.TeamId,
            OverallPick: request.OverallPick
        );

    public ReplaceDraftPickCommand ToCommand(int draftId, int overallPick, ReplaceDraftPickRequest request) => new ReplaceDraftPickCommand(
            DraftId: draftId,
            OverallPick: overallPick,
            NewPlayerId: request.NewPlayerId
        );

    public SetPlayerPoolCommand ToCommand(SetPlayerPoolRequest request) => new SetPlayerPoolCommand(
            ExcludedPlayerIds: request.ExcludedPlayerIds
        );

    DraftPickBriefDto? ToDraftPickBriefDto(DraftPick? draftPick) => draftPick is null
            ? null
            : new DraftPickBriefDto
            {
                DraftId = draftPick.DraftId,
                OverallPick = draftPick.OverallPick,
                Round = draftPick.Round,
                RoundPick = draftPick.RoundPick,
                Team = ToTeamBriefDto(draftPick.Team),
            };
}
