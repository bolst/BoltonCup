namespace BoltonCup.Core;

/// <summary>The scoreboard state of a game, derived from its <see cref="GameState"/>.</summary>
public enum GameScoreStatus
{
    Upcoming,
    Live,
    Final,
}

public static class GameScoreStatusExtensions
{
    public static GameScoreStatus GetScoreStatus(this Game game) => game.GameState switch
    {
        GameState.Completed => GameScoreStatus.Final,
        GameState.InProgress => GameScoreStatus.Live,
        _ => GameScoreStatus.Upcoming,
    };
}