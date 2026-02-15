namespace BoltonCup.Core;

public class Goal : EntityBase
{
    public required int Id { get; set; }
    public required int GameId { get; set; }
    public required int TeamId { get; set; }
    public required int Period { get; set; }
    public required string PeriodLabel  { get; set; }
    public required TimeSpan PeriodTimeRemaining { get; set; }
    public required int GoalPlayerId { get; set; }
    public int? Assist1PlayerId { get; set; }
    public int? Assist2PlayerId { get; set; }
    public string? Notes { get; set; }
    
    public Team Team { get; set; } = null!;
    public Game Game { get; set; } = null!;
    public Player Scorer { get; set; } = null!;
    public Player? Assist1Player { get; set; }
    public Player? Assist2Player { get; set; }
}

public class GoalComparer : IEqualityComparer<Goal>
{
    public bool Equals(Goal? item1, Goal? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Goal item) => item.Id;
}