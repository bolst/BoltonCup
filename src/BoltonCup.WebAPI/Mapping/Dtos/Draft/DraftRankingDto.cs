namespace BoltonCup.WebAPI.Mapping;

public sealed record DraftRankingDto
{
    public int Id { get; set; }
    public int DraftId { get; set; }
    public string? PlayerPhone { get; set; }
    public required PlayerBriefDto Player { get; set; }
    public required TournamentBriefDto Tournament { get; set; }
    public TeamBriefDto? Team { get; set; }
    public int? OverallPick { get; set; }
    public int GamesPlayed { get; set; }
    public int TotalPoints { get; set; }
    public bool IsChampion { get; set; }
    public double DraftRanking { get; set; }
    public bool OverrideRanking { get; set; }
    public bool IsDrafted { get; set; }
    public double PointsPerGame { get; set; }
}