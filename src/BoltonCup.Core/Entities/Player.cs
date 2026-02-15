namespace BoltonCup.Core;

public class Player : EntityBase
{
    public required int Id { get; set; }
    public required int TournamentId { get; set; }
    public required int AccountId { get; set; }
    public int? TeamId { get; set; }
    public string? Position { get; set; }
    public int? JerseyNumber { get; set; }

    public Account Account { get; set; } = null!;
    public Tournament Tournament { get; set; } = null!;
    public Team? Team { get; set; }
    public ICollection<Goal> Goals { get; set; } = [];
    public ICollection <Goal> PrimaryAssists { get; set; } = [];
    public ICollection <Goal> SecondaryAssists { get; set; } = [];
    public ICollection<Penalty> Penalties { get; set; } = [];
    public ICollection<SkaterGameLog> SkaterGameLogs { get; set; } = [];
    public ICollection<GoalieGameLog> GoalieGameLogs { get; set; } = [];
}

public class PlayerComparer : IEqualityComparer<Player>
{
    public bool Equals(Player? item1, Player? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Player item) => item.Id;
}