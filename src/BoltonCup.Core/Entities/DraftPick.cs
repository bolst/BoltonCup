namespace BoltonCup.Core;

public class DraftPick : EntityBase
{
    public int Id { get; set; }
    public int DraftId { get; set; }
    public int OverallPick { get; set; }
    public int TeamId { get; set; }
    public int? PlayerId { get; set; }

    public Draft Draft { get; set; } = null!;
    public Team Team { get; set; } = null!;
    public Player? Player { get; set; }
}