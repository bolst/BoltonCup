namespace BoltonCup.Shared.Data;

public class BCTournament : IEquatable<BCTournament>
{
    public required int tournament_id { get; set; }
    public DateTime? start_date { get; set; }
    public DateTime? end_date { get; set; }
    public int? winning_team_id { get; set; }
    public required string name { get; set; }
    public bool registration_open { get; set; }
    public bool payment_open { get; set; }
    public string? payment_link { get; set; }
    public string? player_payment_link { get; set; }
    public string? goalie_payment_link { get; set; }
    public int player_limit { get; set; }
    public int goalie_limit { get; set; }

    public bool Equals(BCTournament? other) => other is not null && other.tournament_id == tournament_id;
    public override bool Equals(object? obj) => Equals(obj as BCTournament);
    public override int GetHashCode() => tournament_id.GetHashCode();
}