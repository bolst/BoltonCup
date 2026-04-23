namespace BoltonCup.Core;

public sealed class PlayerDraftRanking : EntityBase
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    public int DraftId { get; set; }
    public Draft Draft { get; set; } = null!;
    public int GamesPlayed { get; set; }
    public int TotalPoints { get; set; }
    public bool IsChampion { get; set; }
    public double DraftRanking { get; set; }
    public bool OverrideRanking { get; set; }
    public bool IsDrafted { get; set; }

    public string PlayerName => Player.Account.FirstName + " " + Player.Account.LastName;
    public double PointsPerGame => GamesPlayed == 0 ? 0 : (double)TotalPoints / GamesPlayed;

    public override string ToString()
    {
        return PlayerName;
    }
}

public class PlayerDraftRankingComparer : IEqualityComparer<PlayerDraftRanking>
{
    public bool Equals(PlayerDraftRanking? item1, PlayerDraftRanking? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Player.Id == item2.Player.Id;
    }
        
    public int GetHashCode(PlayerDraftRanking item) => item.Player.Id;
}