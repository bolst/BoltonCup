namespace BoltonCup.Core;

public class Penalty : EntityBase
{
    public required int Id { get; set; }
    public required int GameId { get; set; }
    public required int TeamId { get; set; }
    public required int Period { get; set; }
    public required string PeriodLabel  { get; set; }
    public required TimeSpan PeriodTimeRemaining { get; set; }
    public required int PlayerId { get; set; }
    public required string InfractionName { get; set; }
    public required int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    
    public Team Team { get; set; } = null!;
    public Game Game { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

public class PenaltyComparer : IEqualityComparer<Penalty>
{
    public bool Equals(Penalty? item1, Penalty? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Penalty item) => item.Id;
}