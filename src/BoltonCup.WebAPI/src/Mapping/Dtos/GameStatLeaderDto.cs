namespace BoltonCup.WebAPI.Mapping;

/// <summary>Stat leader pair for a single category (e.g. Points) in a game.</summary>
public record GameStatLeaderDto(string Title, GameStatLeaderPlayerDto? HomeLeader, GameStatLeaderPlayerDto? AwayLeader);