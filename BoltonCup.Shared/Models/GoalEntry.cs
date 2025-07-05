namespace BoltonCup.Shared.Data;

public class GoalEntry
{
    public required BCGame Game { get; set; }
    public required BCTeam Team { get; set; }
    public required PlayerProfile Scorer { get; set; }
    public PlayerProfile? Assist1 { get; set; }
    public PlayerProfile? Assist2 { get; set; }
    public required int Period { get; set; }
    public required TimeSpan Time { get; set; }
}