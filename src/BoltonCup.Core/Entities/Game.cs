namespace BoltonCup.Core;

public class Game : EntityBase
{
    public required int Id { get; set; }
    public required int TournamentId { get; set; }
    public required DateTime GameTime { get; set; }
    public int? HomeTeamId { get; set; }
    public int? AwayTeamId { get; set; }
    public string? GameType { get; set; }
    public string? Venue  { get; set; }
    public string? Rink { get; set; }
    
    public Tournament Tournament { get; set; } = null!;
    public Team? HomeTeam { get; set; }
    public Team? AwayTeam { get; set; }
    public ICollection<Goal> Goals { get; set; } = [];
    public ICollection<Penalty> Penalties { get; set; } = [];
    public ICollection<SkaterGameLog> SkaterGameLogs { get; set; } = [];
    public ICollection<GoalieGameLog> GoalieGameLogs { get; set; } = [];
}

public class GameComparer : IEqualityComparer<Game>
{
    public bool Equals(Game? item1, Game? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Game item) => item.Id;
}