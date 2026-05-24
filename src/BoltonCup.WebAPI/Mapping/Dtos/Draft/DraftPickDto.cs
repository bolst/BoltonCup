namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a draft pick.</summary>
public record DraftPickDto
{
    /// <summary>Gets or sets the ID of the draft this pick belongs to.</summary>
    public required int DraftId { get; set; }
    /// <summary>Gets or sets the overall pick number.</summary>
    public required int OverallPick { get; set; }
    /// <summary>Gets or sets the round number.</summary>
    public required int Round { get; set; }
    /// <summary>Gets or sets the pick number within the round.</summary>
    public required int RoundPick { get; set; }
    /// <summary>Gets or sets the team that holds this pick.</summary>
    public required TeamBriefDto Team { get; set; }
    /// <summary>Gets or sets the player selected with this pick, if any.</summary>
    public PlayerBriefDto? Player { get; set; }
}