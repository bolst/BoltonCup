namespace BoltonCup.Shared.Data;

public class BCGoal
{
    public int id { get; set; }
    public int game_id { get; set; }
    public TimeSpan time { get; set; }
    public int period { get; set; }
    public bool is_hometeam { get; set; }
    public int tournament_id { get; set; }
    public int scorer_id { get; set; }
    public int? assist1_player_id { get; set; }
    public int? assist2_player_id { get; set; }
}