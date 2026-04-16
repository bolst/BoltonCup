namespace BoltonCup.WebAPI.Mapping;

public sealed record GameStarDto(
    int StarRank,
    PlayerBriefDto Player,
    List<StatItem> Stats
);

public sealed record StatItem(string tag, string value);