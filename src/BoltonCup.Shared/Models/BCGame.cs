namespace BoltonCup.Shared.Data;

public class BCGame
{
    public int id { get; set; }
    public int home_team_id { get; set; }
    public int away_team_id { get; set; }
    public DateTime date { get; set; }
    public string type { get; set; } = "";
    public string location { get; set; } = "";
    public string rink { get; set; } = "";
    public int tournament_id { get; set; }
    public int home_score { get; set; }
    public int away_score { get; set; }
    public required string state { get; set; }
    public string? playlist_id { get; set; }
    public string? last_played { get; set; }
    public string? last_played_id { get; set; }
    public string HomeTeamName { get; set; }
    public string HomeTeamLogo { get; set; }
    public string HomeTeamNameShort { get; set; }
    public string AwayTeamName { get; set; }
    public string AwayTeamNameShort { get; set; }
    public string AwayTeamLogo { get; set; }
}