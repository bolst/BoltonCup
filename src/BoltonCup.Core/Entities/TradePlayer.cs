namespace BoltonCup.Core;

public class TradePlayer : EntityBase
{
    public int Id { get; set; }
    public int TradeId { get; set; }
    public required int PlayerId { get; set; }
    public required int FromTeamId { get; set; }
    public required int ToTeamId { get; set; }

    public Trade Trade { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
