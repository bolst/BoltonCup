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