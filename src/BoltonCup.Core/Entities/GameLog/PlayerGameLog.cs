namespace BoltonCup.Core;

// The reason this looks the same as the GoalieGameLog,
// is because a goalie can have every player stat, but not the other way around.
// So we keep them separate for clarity and because we try not to do dumb shit like inheriting a goalie class.
public class PlayerGameLog : GameLogBase
{
    public required int GoalsAgainst { get; set; }
    public required int ShotsAgainst { get; set; }
    public required int Saves { get; set; }
    public required bool Shutout { get; set; }
    public required bool Win { get; set; }
}