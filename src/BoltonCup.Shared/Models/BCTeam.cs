namespace BoltonCup.Shared.Data;

public class BCTeam : IEquatable<BCTeam>
{
    public required int id { get; set; }
    public required string name { get; set; }
    public string name_short { get; set; } = "";
    public required string primary_color_hex { get; set; }
    public required string secondary_color_hex { get; set; }
    public string? tertiary_color_hex { get; set; } = "";
    public string? logo_url { get; set; } = "";
    public required int tournament_id { get; set; }
    public required int gm_account_id { get; set; }
    public string? banner_image { get; set; }
    public string? goal_horn_url { get; set; }
    public string? penalty_song_url { get; set; }

    public bool Equals(BCTeam? other) => other is not null && other.id == id;
    public override bool Equals(object? obj) => Equals(obj as BCTeam);
    public override int GetHashCode() => id.GetHashCode();
}