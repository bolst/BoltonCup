namespace BoltonCup.WebAPI.Mapping;

public sealed record GameStarDto(
    int StarRank,
    PlayerBriefDto Player
);