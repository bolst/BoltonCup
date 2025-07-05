namespace BoltonCup.Shared.Data;

public class GameGoal
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public TimeSpan Time { get; set; }
    public int Period { get; set; }
    public int TeamId { get; set; }
    public int ScorerId { get; set; }
    public int? Assist1Id { get; set; }
    public int? Assist2Id { get; set; }
    public string ScorerName { get; set; }
    public string? Assist1Name { get; set; }
    public string? Assist2Name { get; set; }
    public string ScorerProfilePic { get; set; }
    public string TeamName { get; set; }
    public string TeamLogo { get; set; }
}