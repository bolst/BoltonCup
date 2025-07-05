namespace BoltonCup.Shared.Data;

public class BCSong : IEquatable<BCSong>
{
    public int id { get; set; }
    public string spotify_id { get; set; }
    public string name { get; set; }
    public int account_id { get; set; }
    public string album_cover { get; set; }
    
    #region IEquatable
    
    public bool Equals(BCSong? other) => other is not null && other.id == id;
    public override bool Equals(object? obj) => Equals(obj as BCSong);
    public override int GetHashCode() => id.GetHashCode();
    public static bool operator == (BCSong? left, BCSong? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (BCSong? left, BCSong? right) => left is null ? right is not null : !left.Equals(right);

    #endregion

}