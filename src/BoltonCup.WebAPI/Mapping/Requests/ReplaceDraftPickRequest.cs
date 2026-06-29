namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to replace the player on an already-made draft pick.</summary>
public record ReplaceDraftPickRequest
{
    /// <summary>Gets or sets the ID of the player to assign to the pick.</summary>
    public int NewPlayerId { get; set; }
}