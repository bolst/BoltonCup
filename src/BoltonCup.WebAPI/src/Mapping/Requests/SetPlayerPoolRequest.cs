namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to set a draft's player pool by listing the players excluded from the draft.</summary>
public record SetPlayerPoolRequest
{
    /// <summary>Gets or sets the player IDs excluded from the draft.</summary>
    public List<int> ExcludedPlayerIds { get; set; } = [];
}