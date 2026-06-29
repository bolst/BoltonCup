namespace BoltonCup.Core;

public sealed class CustomRankingShare : EntityBase
{
    public int Id { get; set; }
    public int CustomRankingId { get; set; }
    public CustomRanking CustomRanking { get; set; } = null!;
    public int SharedWithAccountId { get; set; }
    public Account SharedWithAccount { get; set; } = null!;
}
