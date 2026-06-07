namespace BoltonCup.Core;

public class Draft : EntityBase
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public string? Title { get; set; }
    public DateTime? StartDate { get; set; }
    public DraftType Type { get; set; }
    public DraftStatus Status { get; set; }
    public bool IsVisible { get; set; }
    public int? DraftOwnerAccountId { get; set; }
    public int Rounds { get; set; }
    public int Teams { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public Account? DraftOwner { get; set; }
    public ICollection<DraftOrder> DraftOrders { get; set; } = [];
    public ICollection<DraftPick> DraftPicks { get; set; } = [];
}