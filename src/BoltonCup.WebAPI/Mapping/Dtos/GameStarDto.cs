namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a game star award.</summary>
/// <param name="StarRank">The star ranking (1 = first star, 2 = second star, etc.).</param>
/// <param name="Player">The player who received the star.</param>
/// <param name="Stats">The relevant stats for this game star.</param>
public sealed record GameStarDto(
    int StarRank,
    PlayerBriefDto Player,
    List<StatItem> Stats
);

/// <summary>Represents a single displayed statistic.</summary>
/// <param name="tag">The label or category of the statistic.</param>
/// <param name="value">The formatted value of the statistic.</param>
public sealed record StatItem(string tag, string value);