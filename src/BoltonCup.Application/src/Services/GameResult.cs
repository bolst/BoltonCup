using BoltonCup.Core;

namespace BoltonCup.Application.Services;

/// <summary>
/// One team's result in one game, derived from the game's goals. Shared by standings and franchise records so the
/// two cannot drift.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>A forfeit is recorded as a completed 0–0 game, so it counts as a tie.</item>
/// <item>A game is OT/SO when any goal was scored in period 4 or later.</item>
/// <item>Franchise records count OT/SO losses as losses; standings keep their separate OT/SO-loss bucket.</item>
/// </list>
/// </remarks>
readonly record struct GameResult(int GoalsFor, int GoalsAgainst, bool IsOtSo)
{
    public bool IsWin => GoalsFor > GoalsAgainst;
    public bool IsLoss => GoalsFor < GoalsAgainst;
    public bool IsTie => GoalsFor == GoalsAgainst;

    /// <summary>
    /// The result for <paramref name="teamId"/>, or null unless the game is completed, both teams are set and
    /// <paramref name="teamId"/> is one of them.
    /// </summary>
    public static GameResult? For(Game game, int teamId)
    {
        if (game.GameState != GameState.Completed || game.HomeTeamId is not { } homeId || game.AwayTeamId is not { } awayId)
        {
            return null;
        }

        if (teamId != homeId && teamId != awayId)
        {
            return null;
        }

        var opponentId = teamId == homeId ? awayId : homeId;
        return new GameResult(
            game.Goals.Count(g => g.TeamId == teamId),
            game.Goals.Count(g => g.TeamId == opponentId),
            game.Goals.Any(g => g.Period >= 4));
    }
}