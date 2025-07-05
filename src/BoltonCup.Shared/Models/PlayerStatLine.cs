namespace BoltonCup.Shared.Data;

public class PlayerStatLine
{
    public int player_id { get; set; }
    public string player_name { get; set; } = "";
    public int jersey_number { get; set; }
    public string position { get; set; } = "";
    public int? team_id { get; set; }
    public DateTime dob { get; set; }
    public string profilepicture { get; set; } = "";
    public string? team_name { get; set; }
    public string? team_name_short { get; set; }
    public string? team_logo { get; set; }
    public int goals { get; set; }
    public int assists { get; set; }
    public int tournament_id { get; set; }

    private string GetShortName()
    {
        var names = player_name.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));
        var initials = names.SkipLast(1).Select(x => x.ToUpper().First());

        return $"{string.Join("", initials)}. {names.Last()}";
    }

    public string ShortName => GetShortName();
}