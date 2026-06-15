namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to set a draft's player pool: the ordered (included) players and the excluded players.</summary>
public record SetPlayerPoolRequest
{
    /// <summary>Gets or sets the player IDs included in the draft, in their desired ranking order.</summary>
    public List<int> OrderedPlayerIds { get; set; } = [];

    /// <summary>Gets or sets the player IDs excluded from the draft.</summary>
    public List<int> ExcludedPlayerIds { get; set; } = [];
}
