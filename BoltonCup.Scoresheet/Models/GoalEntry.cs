using BoltonCup.Shared.Data;

namespace BoltonCup.Scoresheet.Data;


public class GoalEntry
{
    public required BCTeam Team { get; set; }
    public required PlayerProfile Scorer { get; set; }
    public PlayerProfile? Assist1 { get; set; }
    public PlayerProfile? Assist2 { get; set; }
}