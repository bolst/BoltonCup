using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- Trade ----------

    public IReadOnlyList<TradeDto> ToDtoList(IReadOnlyList<Trade> trades, TradeViewerContext viewer)
        => trades.Select(trade => ToDto(trade, viewer)).ToList();

    public TradeDto ToDto(Trade trade, TradeViewerContext viewer)
    {
        var isProposingGm = viewer.AccountId is { } accountId && trade.ProposingTeam.GeneralManagers.Any(g => g.Id == accountId);
        var isReceivingGm = viewer.AccountId is { } accId && trade.ReceivingTeam.GeneralManagers.Any(g => g.Id == accId);

        return new TradeDto
        {
            Id = trade.Id,
            TournamentId = trade.TournamentId,
            ProposingTeam = ToTeamBriefDto(trade.ProposingTeam),
            ReceivingTeam = ToTeamBriefDto(trade.ReceivingTeam),
            Status = trade.Status,
            Note = trade.Note,
            CreatedAt = trade.CreatedAt,
            RespondedAt = trade.RespondedAt,
            ResolvedAt = trade.ResolvedAt,
            PlayersFromProposing = trade.Players
                .Where(tp => tp.FromTeamId == trade.ProposingTeamId)
                .Select(ToTradePlayerDto)
                .ToList(),
            PlayersFromReceiving = trade.Players
                .Where(tp => tp.FromTeamId == trade.ReceivingTeamId)
                .Select(ToTradePlayerDto)
                .ToList(),
            CanAccept = isReceivingGm && trade.Status == TradeStatus.Pending,
            CanDecline = isReceivingGm && trade.Status == TradeStatus.Pending,
            CanCancel = (isProposingGm && trade.Status == TradeStatus.Pending) || (viewer.IsAdmin && trade.Status is TradeStatus.Pending or TradeStatus.Accepted),
            CanApprove = viewer.IsAdmin && trade.Status == TradeStatus.Accepted,
        };
    }

    TradePlayerDto ToTradePlayerDto(TradePlayer tradePlayer) => new TradePlayerDto
    {
        Player = ToPlayerBriefDto(tradePlayer.Player),
        FromTeamId = tradePlayer.FromTeamId,
        ToTeamId = tradePlayer.ToTeamId,
    };

    public CreateTradeCommand ToCommand(CreateTradeRequest request, ClaimsPrincipal user) => new CreateTradeCommand(TournamentId: request.TournamentId, ProposingTeamId: request.ProposingTeamId, ReceivingTeamId: request.ReceivingTeamId, ProposingPlayerIds: request.ProposingPlayerIds, ReceivingPlayerIds: request.ReceivingPlayerIds, Note: request.Note, CreatedByAccountId: user.GetAccountId());
}
