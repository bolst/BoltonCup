using BoltonCup.Core;
using BoltonCup.Core.Values;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

/// <summary>
/// Recomputes the skater/goalie game-log tables from the underlying game data. The derivation used to
/// live in the <c>mv_skater_game_logs</c>/<c>mv_goalie_game_logs</c> materialized views; it now runs in
/// memory here and the results are upserted into the <c>skater_game_logs</c>/<c>goalie_game_logs</c>
/// tables so every process reads the same DB-backed data without a cache. Uses a short-lived context
/// from the factory so it is safe to call from the Blazor Server Admin app as well as the API.
/// </summary>
public sealed class StatisticsRefreshService(IDbContextFactory<BoltonCupDbContext> _dbContextFactory)
    : IStatisticsRefreshService
{
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var (skaterRows, goalieRows) = await ComputeGameLogsAsync(db, cancellationToken);

        // Replace the tables' contents atomically. Delete first (which also detaches the tracked rows)
        // then insert the freshly computed rows so the composite (game_id, player_id) keys don't clash.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        db.SkaterStats.RemoveRange(await db.SkaterStats.ToListAsync(cancellationToken));
        db.GoalieStats.RemoveRange(await db.GoalieStats.ToListAsync(cancellationToken));
        await db.SaveChangesAsync(cancellationToken);

        db.SkaterStats.AddRange(skaterRows);
        db.GoalieStats.AddRange(goalieRows);
        await db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Builds the per-player-per-game "game log" rows in memory, mirroring the SQL the
    /// <c>mv_skater_game_logs</c>/<c>mv_goalie_game_logs</c> materialized views used to run.
    /// </summary>
    private static async Task<(List<SkaterStat> Skaters, List<GoalieStat> Goalies)> ComputeGameLogsAsync(
        BoltonCupDbContext db,
        CancellationToken cancellationToken)
    {
        // A single tournament's worth of data — small enough to pull entirely and aggregate in memory.
        var games = await db.Games.AsNoTracking().ToListAsync(cancellationToken);
        var players = await db.Players.AsNoTracking().ToListAsync(cancellationToken);
        var accounts = await db.Accounts.AsNoTracking().ToListAsync(cancellationToken);
        var teams = await db.Teams.AsNoTracking().ToListAsync(cancellationToken);
        var tournaments = await db.Tournaments.AsNoTracking().ToListAsync(cancellationToken);
        var goals = await db.Goals.AsNoTracking().ToListAsync(cancellationToken);
        var penalties = await db.Penalties.AsNoTracking().ToListAsync(cancellationToken);

        var accountsById = accounts.ToDictionary(a => a.Id);
        var teamsById = teams.ToDictionary(t => t.Id);
        var tournamentsById = tournaments.ToDictionary(t => t.Id);

        // Per-(game, player) goals/assists/penalty minutes and per-(game, team) goal totals. Assists
        // count assist1 and assist2 separately (a UNION ALL in the view), matching the old SQL.
        var goalsByGamePlayer = goals
            .GroupBy(g => (g.GameId, g.GoalPlayerId))
            .ToDictionary(g => g.Key, g => g.Count());
        var assistsByGamePlayer = goals
            .SelectMany(g => new[] { g.Assist1PlayerId, g.Assist2PlayerId }
                .Where(id => id.HasValue)
                .Select(id => (g.GameId, PlayerId: id!.Value)))
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => x.Count());
        var penaltyMinutesByGamePlayer = penalties
            .GroupBy(p => (p.GameId, p.PlayerId))
            .ToDictionary(p => p.Key, p => (double)p.Sum(x => x.DurationMinutes));
        var goalsByGameTeam = goals
            .GroupBy(g => (g.GameId, g.TeamId))
            .ToDictionary(g => g.Key, g => g.Count());
        // Empty-net goals per (game, scoring team). Excluded from the conceding goalie's GAA (but not
        // from raw GoalsAgainst) since they're scored after that team pulled its goalie.
        var emptyNetGoalsByGameTeam = goals
            .Where(g => g.IsEmptyNetGoal)
            .GroupBy(g => (g.GameId, g.TeamId))
            .ToDictionary(g => g.Key, g => g.Count());

        var skaterRows = new List<SkaterStat>();
        var goalieRows = new List<GoalieStat>();

        foreach (var game in games)
        {
            // The view's roster joins players to games by team membership, and both team + opponent are
            // INNER-joined, so a game with an unassigned side contributes nothing.
            if (game.HomeTeamId is not { } homeTeamId || game.AwayTeamId is not { } awayTeamId)
            {
                continue;
            }

            if (!tournamentsById.TryGetValue(game.TournamentId, out var tournament))
            {
                continue;
            }

            var gameType = EnumMemberConverter<GameType>.GetEnumMemberValue(game.GameType);

            foreach (var player in players)
            {
                if (player.TeamId is not { } teamId || (teamId != homeTeamId && teamId != awayTeamId))
                {
                    continue;
                }

                // The view excludes players with no position from both rosters (NULL <> 'goalie' and
                // NULL = 'goalie' are both untrue), and classifies goalies by an exact 'goalie' match.
                if (player.Position is not { } position)
                {
                    continue;
                }

                var opponentId = teamId == homeTeamId ? awayTeamId : homeTeamId;
                if (!teamsById.TryGetValue(teamId, out var team)
                    || !teamsById.TryGetValue(opponentId, out var opponent)
                    || !accountsById.TryGetValue(player.AccountId, out var account))
                {
                    continue;
                }

                var goalsScored = goalsByGamePlayer.GetValueOrDefault((game.Id, player.Id));
                var assists = assistsByGamePlayer.GetValueOrDefault((game.Id, player.Id));
                var penaltyMinutes = penaltyMinutesByGamePlayer.GetValueOrDefault((game.Id, player.Id));

                if (string.Equals(position, Position.Goalie, StringComparison.Ordinal))
                {
                    var goalsAgainst = goalsByGameTeam.GetValueOrDefault((game.Id, opponentId));
                    var ownGoals = goalsByGameTeam.GetValueOrDefault((game.Id, teamId));
                    // Empty-net goals against don't count toward GAA (goalie was pulled), but still
                    // count in raw GoalsAgainst / Shutouts / Wins.
                    var emptyNetAgainst = emptyNetGoalsByGameTeam.GetValueOrDefault((game.Id, opponentId));

                    goalieRows.Add(new GoalieStat
                    {
                        PlayerId = player.Id,
                        GoalsAgainst = goalsAgainst,
                        // Shots/saves aren't tracked, so these mirror the view's hardcoded values.
                        ShotsAgainst = 0,
                        Saves = 0,
                        Shutouts = goalsAgainst == 0 ? 1 : 0,
                        Wins = ownGoals > goalsAgainst ? 1 : 0,
                        SavePercentage = 0.0,
                        GoalsAgainstAverage = goalsAgainst - emptyNetAgainst,
                        GamesPlayed = 1,
                        Goals = goalsScored,
                        Assists = assists,
                        Points = goalsScored + assists,
                        PenaltyMinutes = penaltyMinutes,
                        AccountId = account.Id,
                        FirstName = account.FirstName,
                        LastName = account.LastName,
                        Position = position,
                        JerseyNumber = player.JerseyNumber,
                        Birthday = account.Birthday,
                        ProfilePicture = account.Avatar,
                        TeamId = teamId,
                        TeamName = team.Name,
                        TeamNameShort = team.NameShort,
                        TeamAbbreviation = team.Abbreviation,
                        TeamLogoUrl = team.Logo,
                        OpponentId = opponentId,
                        OpponentName = opponent.Name,
                        OpponentNameShort = opponent.NameShort,
                        OpponentAbbreviation = opponent.Abbreviation,
                        OpponentLogoUrl = opponent.Logo,
                        GameId = game.Id,
                        GameTime = game.GameTime,
                        GameType = gameType,
                        GameVenue = game.Venue,
                        GameRink = game.Rink,
                        TournamentId = game.TournamentId,
                        TournamentName = tournament.Name,
                        TournamentActive = tournament.IsActive,
                    });
                }
                else
                {
                    skaterRows.Add(new SkaterStat
                    {
                        PlayerId = player.Id,
                        GamesPlayed = 1,
                        Goals = goalsScored,
                        Assists = assists,
                        Points = goalsScored + assists,
                        PenaltyMinutes = penaltyMinutes,
                        AccountId = account.Id,
                        FirstName = account.FirstName,
                        LastName = account.LastName,
                        Position = position,
                        JerseyNumber = player.JerseyNumber,
                        Birthday = account.Birthday,
                        ProfilePicture = account.Avatar,
                        TeamId = teamId,
                        TeamName = team.Name,
                        TeamNameShort = team.NameShort,
                        TeamAbbreviation = team.Abbreviation,
                        TeamLogoUrl = team.Logo,
                        OpponentId = opponentId,
                        OpponentName = opponent.Name,
                        OpponentNameShort = opponent.NameShort,
                        OpponentAbbreviation = opponent.Abbreviation,
                        OpponentLogoUrl = opponent.Logo,
                        GameId = game.Id,
                        GameTime = game.GameTime,
                        GameType = gameType,
                        GameVenue = game.Venue,
                        GameRink = game.Rink,
                        TournamentId = game.TournamentId,
                        TournamentName = tournament.Name,
                        TournamentActive = tournament.IsActive,
                    });
                }
            }
        }

        return (skaterRows, goalieRows);
    }
}
