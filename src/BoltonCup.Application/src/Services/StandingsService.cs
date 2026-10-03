using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

class StandingsService(BoltonCupDbContext _context) : IStandingsService
{
    public async Task<TournamentStandings> GetStandingsAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        var teams = await _context.Teams
            .AsNoTracking()
            .Where(t => t.TournamentId == tournamentId)
            .ToListAsync(cancellationToken);

        var games = await _context.Games
            .AsNoTracking()
            .Where(g => g.TournamentId == tournamentId && g.GameState == GameState.Completed)
            .Include(g => g.Goals)
            .ToListAsync(cancellationToken);

        return new TournamentStandings
        {
            RoundRobin = Compute(teams, games, StandingsStage.RoundRobin, StandingsRules.Default),
            Playoffs = Compute(teams, games, StandingsStage.Playoffs, StandingsRules.Default),
        };
    }

    /// <summary>
    /// Pure, DB-free standings computation: accumulates per-team results for completed games in the
    /// given stage and ranks them by Points → head-to-head → regulation wins → goal differential → goals for.
    /// </summary>
    public static IReadOnlyList<StandingRow> Compute(
        IEnumerable<Team> teams,
        IEnumerable<Game> games,
        StandingsStage stage,
        StandingsRules rules)
    {
        var rows = teams.ToDictionary(t => t.Id, t => new StandingRow { Team = t });

        var stageGames = games
            .Where(g => g.GameState == GameState.Completed)
            .Where(g => g.HomeTeamId.HasValue && g.AwayTeamId.HasValue)
            .Where(g => g.GameType.ToStandingsStage() == stage)
            .ToList();

        foreach (var game in stageGames)
        {
            var homeId = game.HomeTeamId!.Value;
            var awayId = game.AwayTeamId!.Value;
            if (!rows.TryGetValue(homeId, out var home) || !rows.TryGetValue(awayId, out var away))
            {
                continue;
            }

            if (GameResult.For(game, homeId) is not { } homeResult || GameResult.For(game, awayId) is not { } awayResult)
            {
                continue;
            }

            ApplyResult(home, homeResult, rules);
            ApplyResult(away, awayResult, rules);
        }

        return Rank(rows.Values, stageGames, rules);
    }

    static void ApplyResult(StandingRow row, GameResult result, StandingsRules rules)
    {
        row.GamesPlayed++;
        row.GoalsFor += result.GoalsFor;
        row.GoalsAgainst += result.GoalsAgainst;

        if (result.IsWin)
        {
            row.Wins++;
            if (result.IsOtSo)
            {
                row.Points += rules.OtSoWin;
            }
            else
            {
                row.RegulationWins++;
                row.Points += rules.RegulationWin;
            }
        }
        else if (result.IsLoss)
        {
            if (result.IsOtSo)
            {
                row.OtSoLosses++;
                row.Points += rules.OtSoLoss;
            }
            else
            {
                row.Losses++;
                row.Points += rules.RegulationLoss;
            }
        }
        else
        {
            row.Ties++;
            row.Points += rules.Tie;
        }
    }

    static IReadOnlyList<StandingRow> Rank(
        IEnumerable<StandingRow> rows,
        IReadOnlyList<Game> stageGames,
        StandingsRules rules)
    {
        var result = new List<StandingRow>();

        foreach (var group in rows.GroupBy(r => r.Points).OrderByDescending(g => g.Key))
        {
            var groupRows = group.ToList();
            if (groupRows.Count == 1)
            {
                result.Add(groupRows[0]);
                continue;
            }

            var headToHead = HeadToHeadPoints(groupRows, stageGames, rules);
            result.AddRange(groupRows
                .OrderByDescending(r => headToHead[r.TeamId])
                .ThenByDescending(r => r.RegulationWins)
                .ThenByDescending(r => r.GoalDifferential)
                .ThenByDescending(r => r.GoalsFor)
                .ThenBy(r => r.Team.Name, StringComparer.OrdinalIgnoreCase));
        }

        for (var i = 0; i < result.Count; i++)
        {
            result[i].Rank = i + 1;
        }

        return result;
    }

    /// <summary>Points each tied team earned only in games played among the tied group (mini-table).</summary>
    static Dictionary<int, int> HeadToHeadPoints(
        IReadOnlyCollection<StandingRow> groupRows,
        IReadOnlyList<Game> stageGames,
        StandingsRules rules)
    {
        var ids = groupRows.Select(r => r.TeamId).ToHashSet();
        var points = ids.ToDictionary(id => id, _ => 0);

        foreach (var game in stageGames)
        {
            var homeId = game.HomeTeamId!.Value;
            var awayId = game.AwayTeamId!.Value;
            if (!ids.Contains(homeId) || !ids.Contains(awayId))
            {
                continue;
            }

            if (GameResult.For(game, homeId) is not { } homeResult || GameResult.For(game, awayId) is not { } awayResult)
            {
                continue;
            }

            points[homeId] += PointsFor(homeResult, rules);
            points[awayId] += PointsFor(awayResult, rules);
        }

        return points;
    }

    static int PointsFor(GameResult result, StandingsRules rules)
        => result.IsWin ? (result.IsOtSo ? rules.OtSoWin : rules.RegulationWin)
            : result.IsLoss ? (result.IsOtSo ? rules.OtSoLoss : rules.RegulationLoss)
            : rules.Tie;
}