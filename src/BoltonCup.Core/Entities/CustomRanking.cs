namespace BoltonCup.Core;

public sealed class CustomRanking : EntityBase
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    public string Title { get; set; } = null!;

    public ICollection<CustomRankingPlayer> Players { get; set; } = [];
    public ICollection<CustomRankingShare> SharedWith { get; set; } = [];
}
