namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to record a draft pick selection.</summary>
public record DraftPlayerRequest
{
    /// <summary>Gets or sets the ID of the player being drafted.</summary>
    public int PlayerId { get; set; }

    /// <summary>Gets or sets the ID of the team making the pick.</summary>
    public int TeamId { get; set; }

    /// <summary>Gets or sets the overall pick number in the draft.</summary>
    public int OverallPick { get; set; }
}