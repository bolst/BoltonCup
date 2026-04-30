namespace BoltonCup.WebAPI.Mapping;

public sealed record DraftRankingDto
{
    public int Id { get; set; }
    public int DraftId { get; set; }
    public required int TournamentId { get; set; }
    public string? PlayerPhone { get; set; }
    public required PlayerBriefDto Player { get; set; }
    public DraftPickBriefDto? DraftPick { get; set; }
    public int GamesPlayed { get; set; }
    public int TotalPoints { get; set; }
    public double DraftRanking { get; set; }
    public bool OverrideRanking { get; set; }
    public bool IsDrafted { get; set; }
    public double PointsPerGame { get; set; }
}