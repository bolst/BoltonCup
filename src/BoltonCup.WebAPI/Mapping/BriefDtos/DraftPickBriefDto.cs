namespace BoltonCup.WebAPI.Mapping;

public record DraftPickBriefDto
{
    public int DraftId { get; set; }
    public int OverallPick { get; set; }
    public int Round { get; set; }
    public int RoundPick { get; set; }
    public TeamBriefDto Team { get; set; } = null!;
}