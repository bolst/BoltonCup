namespace BoltonCup.WebAPI.Mapping;

public record DraftPickDto
{
    public required int DraftId { get; set; }
    public required int OverallPick { get; set; }
    public required int Round { get; set; }
    public required int RoundPick { get; set; }

    public required TeamBriefDto Team { get; set; }
    public PlayerBriefDto? Player { get; set; }
}