using BoltonCup.Core;

namespace BoltonCup.Application.Services;

/// <summary>Per-season records, their all-time sum, and the number of games between two teams of the franchise.</summary>
sealed record FranchiseRecords(IReadOnlyDictionary<int, FranchiseRecord> ByTeam, FranchiseRecord AllTime, int IntraFranchiseGames);

/// <summary>
/// Pure, DB-free franchise record computation over completed games of every type. Each team-season is summed on
/// its own, so a game between two teams of the franchise adds one result per side and the season rows sum exactly
/// to the all-time line.
/// </summary>
static class FranchiseRecordCalculator
{
    /// <param name="teams">The franchise's team rows (one per season).</param>
    /// <param name="games">Distinct games involving any of <paramref name="teams"/>; incomplete games are ignored.</param>
    public static FranchiseRecords Compute(IEnumerable<Team> teams, IEnumerable<Game> games)
    {
        var teamIds = teams.Select(t => t.Id).ToHashSet();
        var gameList = games.ToList();
        var byTeam = new Dictionary<int, FranchiseRecord>();
        var allTime = FranchiseRecord.Empty;

        foreach (var teamId in teamIds)
        {
            var record = FranchiseRecord.Empty;
            foreach (var game in gameList)
            {
                if (GameResult.For(game, teamId) is { } result)
                {
                    record = record.Add(ToRecord(result));
                }
            }
            byTeam[teamId] = record;
            allTime = allTime.Add(record);
        }

        var intraFranchiseGames = gameList.Count(g =>
            g is { HomeTeamId: { } homeId, AwayTeamId: { } awayId }
            && teamIds.Contains(homeId)
            && teamIds.Contains(awayId)
            && GameResult.For(g, homeId) is not null);

        return new FranchiseRecords(byTeam, allTime, intraFranchiseGames);
    }

    static FranchiseRecord ToRecord(GameResult result) => new(
        GamesPlayed: 1,
        Wins: result.IsWin ? 1 : 0,
        Losses: result.IsLoss ? 1 : 0,
        Ties: result.IsTie ? 1 : 0,
        GoalsFor: result.GoalsFor,
        GoalsAgainst: result.GoalsAgainst);
}