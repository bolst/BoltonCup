namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to set the 1st/2nd/3rd stars for a game. Replaces any existing stars.</summary>
public record SetGameStarsRequest
{
    /// <summary>Gets or sets the ID of the first star player, if any.</summary>
    public int? FirstStarPlayerId { get; set; }

    /// <summary>Gets or sets the ID of the second star player, if any.</summary>
    public int? SecondStarPlayerId { get; set; }

    /// <summary>Gets or sets the ID of the third star player, if any.</summary>
    public int? ThirdStarPlayerId { get; set; }
}
