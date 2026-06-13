namespace BoltonCup.Core;

/// <summary>The stage a game belongs to for standings purposes.</summary>
public enum StandingsStage
{
    RoundRobin,
    Playoffs,
}

public static class StandingsStageExtensions
{
    /// <summary>Classifies a <see cref="GameType"/> into a standings stage.</summary>
    public static StandingsStage ToStandingsStage(this GameType gameType) =>
        gameType == GameType.RoundRobin ? StandingsStage.RoundRobin : StandingsStage.Playoffs;
}
