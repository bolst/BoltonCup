namespace BoltonCup.Core;

public class Draft : EntityBase
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public string? Title { get; set; }
    public DateTime? StartDate { get; set; }
    public DraftType Type { get; set; }
    public DraftStatus Status { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public ICollection<DraftOrder> DraftOrders { get; set; } = [];
    public ICollection<DraftPick> DraftPicks { get; set; } = [];
}