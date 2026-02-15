namespace BoltonCup.Core;

public class Tournament : EntityBase
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? WinningTeamId { get; set; }
    public bool IsActive { get; set; }
    public bool IsRegistrationOpen { get; set; }
    public bool IsPaymentOpen { get; set; }
    public string? SkaterPaymentLink { get; set; }
    public string? GoaliePaymentLink { get; set; }
    public int? SkaterLimit { get; set; }
    public int? GoalieLimit { get; set; }

    public ICollection<Player> Players { get; set; } = [];
    public ICollection<Team> Teams { get; set; } = [];
    public ICollection<Game> Games { get; set; } = [];
}

public class TournamentComparer : IEqualityComparer<Tournament>
{
    public bool Equals(Tournament? item1, Tournament? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Tournament item) => item.Id;
}