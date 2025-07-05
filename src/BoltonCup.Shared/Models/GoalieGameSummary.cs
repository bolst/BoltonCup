namespace BoltonCup.Shared.Data;

public class GoalieGameSummary : BCGame
{
    public int team_id { get; set; }
    public string team_name { get; set; }
    public string team_name_short { get; set; }
    public string team_logo_url { get; set; }
    public int? team_score { get; set; }
    public int opponent_team_id { get; set; }
    public string opponent_name { get; set; }
    public string opponent_name_short { get; set; }
    public string opponent_logo_url { get; set; }
    public int? opponent_team_score { get; set; }
    public int player_id { get; set; }
    public bool win { get; set; }
}