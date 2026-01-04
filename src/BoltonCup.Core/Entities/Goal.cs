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
    
    public Team Team { get; set; }
    public Game Game { get; set; }
    public Player Scorer { get; set; }
    public Player? Assist1Player { get; set; }
    public Player? Assist2Player { get; set; }
}