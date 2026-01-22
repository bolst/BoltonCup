namespace BoltonCup.Core;

public class GoalieGameLog : GameLogBase
{
    public required int GoalsAgainst { get; set; }
    public required int ShotsAgainst { get; set; }
    public required int Saves { get; set; }
    public required bool Shutout { get; set; }
    public required bool Win { get; set; }
}