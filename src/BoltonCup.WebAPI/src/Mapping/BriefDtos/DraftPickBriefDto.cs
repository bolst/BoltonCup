namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of a draft pick.</summary>
public record DraftPickBriefDto
{
    /// <summary>Gets or sets the ID of the draft this pick belongs to.</summary>
    public int DraftId { get; set; }
    /// <summary>Gets or sets the overall pick number.</summary>
    public int OverallPick { get; set; }
    /// <summary>Gets or sets the round number.</summary>
    public int Round { get; set; }
    /// <summary>Gets or sets the pick number within the round.</summary>
    public int RoundPick { get; set; }
    /// <summary>Gets or sets the team that made this pick.</summary>
    public TeamBriefDto Team { get; set; } = null!;
}