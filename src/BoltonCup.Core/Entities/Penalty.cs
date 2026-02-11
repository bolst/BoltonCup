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