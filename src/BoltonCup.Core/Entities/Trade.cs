namespace BoltonCup.Core;

public class Trade : EntityBase
{
    public int Id { get; set; }
    public required int TournamentId { get; set; }
    public required int ProposingTeamId { get; set; }
    public required int ReceivingTeamId { get; set; }
    public TradeStatus Status { get; set; } = TradeStatus.Pending;
    public string? Note { get; set; }

    public int CreatedByAccountId { get; set; }
    public int? RespondedByAccountId { get; set; }
    public DateTime? RespondedAt { get; set; }
    public int? ResolvedByAccountId { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public Team ProposingTeam { get; set; } = null!;
    public Team ReceivingTeam { get; set; } = null!;
    public ICollection<TradePlayer> Players { get; set; } = [];
}
