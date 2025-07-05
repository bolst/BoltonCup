namespace BoltonCup.Shared.Data;

public class PlayerProfile : IEquatable<PlayerProfile>
{
    public int id { get; set; }
    public required string name { get; set; }
    public DateTime dob { get; set; }
    public string position { get; set; } = "";
    public int? jersey_number { get; set; }
    public int? account_id { get; set; }
    public int? team_id { get; set; }
    public bool champion { get; set; }
    public int tournament_id { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Birthday { get; set; }
    public string? HighestLevel { get; set; }
    public string? ProfilePicture { get; set; }
    
    public bool IsForward => position == "forward";
    public bool IsDefense => position == "defense";
    public bool IsGoalie => position == "goalie";
    
    
    #region IEquatable
    
    public bool Equals(PlayerProfile? other) => other is not null && other.id == id;
    public override bool Equals(object? obj) => Equals(obj as PlayerProfile);
    public override int GetHashCode() => id.GetHashCode();
    public static bool operator == (PlayerProfile? left, PlayerProfile? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (PlayerProfile? left, PlayerProfile? right) => left is null ? right is not null : !left.Equals(right);

    #endregion
}