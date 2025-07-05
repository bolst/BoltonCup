namespace BoltonCup.Shared.Data;

public class BCAvailability : IEquatable<BCAvailability>
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int GameId { get; set; }
    public string? Availability { get; set; }
    public DateTime GameDate { get; set; }
    public int TeamId { get; set; }
    public int OpponentId { get; set; }
    public string TeamName { get; set; }
    public string OpponentName { get; set; }
    public string TeamLogo { get; set; }
    public string OpponentLogo { get; set; }
    public string GameType { get; set; }
    
    #region IEquatable
    
    public bool Equals(BCAvailability? other) => other is not null && other.Id == Id;
    public override bool Equals(object? obj) => Equals(obj as BCAvailability);
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator == (BCAvailability? left, BCAvailability? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (BCAvailability? left, BCAvailability? right) => left is null ? right is not null : !left.Equals(right);

    #endregion
}