namespace BoltonCup.Core;

public class TradePlayer : EntityBase
{
    public int Id { get; set; }
    public int TradeId { get; set; }
    public required int PlayerId { get; set; }
    public required int FromTeamId { get; set; }
    public required int ToTeamId { get; set; }

    /// <summary>
    /// True while the parent trade locks this player (Pending/Accepted); false once the trade reaches a terminal state.
    /// Backs the partial unique index that prevents a player from being in two open trades at once.
    /// </summary>
    public bool IsLocked { get; set; } = true;

    public Trade Trade { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
