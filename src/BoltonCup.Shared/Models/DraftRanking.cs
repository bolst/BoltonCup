namespace BoltonCup.Shared.Data;

public class DraftRanking : IEquatable<DraftRanking>
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; }
    public int AccountId { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamLogo { get; set; }
    public string Position { get; set; }
    public int Rank { get; set; }
    
    #region IEquatable
    
    public bool Equals(DraftRanking? other) => other is not null && other.PlayerId == PlayerId;
    public override bool Equals(object? obj) => Equals(obj as DraftRanking);
    public override int GetHashCode() => PlayerId.GetHashCode();
    public static bool operator == (DraftRanking? left, DraftRanking? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (DraftRanking? left, DraftRanking? right) => left is null ? right is not null : !left.Equals(right);

    #endregion

}