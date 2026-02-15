namespace BoltonCup.Core;

public abstract class GameLogBase : EntityBase
{
    public required int Id { get; set; }
    public required int PlayerId { get; set; }
    public required int TeamId { get; set; }
    public required int OpponentTeamId { get; set; }
    public required int GameId { get; set; }
    public required int Goals { get; set; }
    public required int Assists { get; set; }
    public required double PenaltyMinutes { get; set; }
    
    public Player Player { get; set; } = null!;
    public Team Team { get; set; } = null!;
    public Team OpponentTeam { get; set; } = null!;
    public Game Game { get; set; } = null!;
}

public class GameLogComparer : IEqualityComparer<GameLogBase>
{
    public bool Equals(GameLogBase? item1, GameLogBase? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(GameLogBase item) => item.Id;
}