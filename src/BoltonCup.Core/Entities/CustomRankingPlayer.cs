namespace BoltonCup.Core;

public sealed class CustomRankingPlayer : EntityBase
{
    public int Id { get; set; }
    public int CustomRankingId { get; set; }
    public CustomRanking CustomRanking { get; set; } = null!;
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public int Rank { get; set; }
    public int GamesPlayed { get; set; }
    public int TotalPoints { get; set; }

    public double PointsPerGame => GamesPlayed == 0 ? 0 : (double)TotalPoints / GamesPlayed;
}
