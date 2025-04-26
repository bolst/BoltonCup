namespace BoltonCup.Shared.Data;
using Dapper;
using Npgsql;

public interface IBCData
{
    Task<IEnumerable<BCTeam>> GetTeams();
    Task<BCTeam?> GetTeamById(int id);
    Task<IEnumerable<BCGame>> GetSchedule();
    Task<BCGame?> GetGameById(int id);
    Task<IEnumerable<PlayerProfile>> GetRosterByTeamId(int teamId);
    Task<IEnumerable<PlayerProfile>> GetAllTournamentPlayersAsync(int tournamentId);
    Task<PlayerProfile?> GetPlayerProfileById(int playerId);
    Task<IEnumerable<PlayerGameSummary>> GetPlayerGameByGame(int playerId);
    Task<IEnumerable<GoalieGameSummary>> GetGoalieGameByGame(int goalieId);
    Task<IEnumerable<GameGoal>> GetGameGoalsByGameId(int gameId);
    Task<IEnumerable<GamePenalty>> GetGamePenaltiesByGameId(int gameId);
    Task<IEnumerable<PlayerStatLine>> GetPlayerStats();
    Task<IEnumerable<GoalieStatLine>> GetGoalieStats();
    Task<IEnumerable<PlayerProfilePicture>> GetPlayerProfilePictures();
    Task<PlayerProfilePicture> GetPlayerProfilePictureById(int playerId);
    Task<string> SubmitRegistration(RegisterFormModel form);
    Task<IEnumerable<RegisterFormModel>> GetRegistrationsAsync();
    Task<RegisterFormModel?> GetRegistrationByEmailAsync(string email);
    Task<string> AdmitUserAsync(RegisterFormModel form);
    Task<IEnumerable<BCAccount>> GetAccountsAsync();
    Task<BCAccount?> GetAccountByEmailAsync(string email);
    Task UpdateAccountProfilePictureAsync(string email, string imagePath);
    Task<IEnumerable<BCTournament>> GetTournamentsAsync();
    Task<BCTournament?> GetTournamentByYearAsync(string year);
    Task SetUserAsPayedAsync(string email);
    Task<BCDraftPick?> GetMostRecentDraftPickAsync(int draftId);
    Task<BCTeam?> GetTeamByDraftOrderAsync(int draftId, int order);
    Task<IEnumerable<BCTeam>> GetTeamsInTournamentAsync(int tournamentId);
    Task<IEnumerable<BCDraftOrder>> GetDraftOrderAsync(int draftId);
    Task DraftPlayerAsync(PlayerProfile player, BCTeam team, BCDraftPick draftPick);
    Task<IEnumerable<BCDraftPickDetail>> GetDraftPicksAsync(int draftId);
    Task ResetDraftAsync(int draftId);
}

public class BCData : IBCData
{
    private readonly string connectionString;
    private readonly ICacheService cacheService;
    private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);


    public BCData(string _connectionString, ICacheService _cacheService)
    {
        connectionString = _connectionString;
        cacheService = _cacheService;
    }

    public async Task<IEnumerable<BCTeam>> GetTeams()
    {
        string cacheKey = "teams";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT *
                        FROM
                          team T";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<BCTeam>(sql);
        }, cacheDuration);
    }

    public async Task<BCTeam?> GetTeamById(int id)
    {
        string cacheKey = $"team_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT *
                        FROM
                            team T
                        WHERE
                            T.id = @TeamId";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<BCTeam>(sql, new { TeamId = id });
        }, cacheDuration);
    }

    public async Task<IEnumerable<BCGame>> GetSchedule()
    {
        string cacheKey = "schedule";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {

            string sql = @"SELECT *
                            FROM
                                game G
                            ORDER BY G.date ASC";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<BCGame>(sql);
        }, cacheDuration);
    }

    public async Task<BCGame?> GetGameById(int id)
    {
        string cacheKey = $"game_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT *
                            FROM
                                game
                            WHERE id = @GameId";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<BCGame>(sql, new { GameId = id });
        }, cacheDuration);
    }

    public async Task<IEnumerable<PlayerProfile>> GetRosterByTeamId(int id)
    {
        string sql = @"SELECT *, p.id
                        FROM players p
                            LEFT OUTER JOIN account a ON p.account_id = a.id AND a.isactive = TRUE
                        WHERE p.team_id = @TeamId";

        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<PlayerProfile>(sql, new { TeamId = id });
    }

    public async Task<IEnumerable<PlayerProfile>> GetAllTournamentPlayersAsync(int tournamentId)
    {
        string sql = @"SELECT *, p.id
                        FROM players p
                                 LEFT OUTER JOIN account a ON p.account_id = a.id AND a.isactive = TRUE
                        WHERE tournament_id = @TournamentId";
        
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<PlayerProfile>(sql, new { TournamentId = tournamentId });
    }
    
    public async Task<PlayerProfile?> GetPlayerProfileById(int id)
    {
        string cacheKey = $"teamplayer_{id}";

        string sql = @"SELECT *, p.id
                        FROM players p 
                            LEFT OUTER JOIN account a ON p.account_id = a.id AND a.isactive = TRUE
                        WHERE p.id = @PlayerId";

        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<PlayerProfile>(sql, new { PlayerId = id });
    }

    public async Task<IEnumerable<PlayerGameSummary>> GetPlayerGameByGame(int id)
    {
        string cacheKey = $"player_gbg_{id}";

        string sql = @"WITH player_teams AS (SELECT *
                          FROM players
                          WHERE id = @PlayerId),
                     player_games AS (SELECT g.*,
                                             p.team_id,
                                             CASE
                                                 WHEN p.team_id = g.home_team_id THEN g.away_team_id
                                                 ELSE g.home_team_id END AS opponent_team_id,
                                             CASE
                                                 WHEN p.team_id = g.home_team_id THEN g.home_score
                                                 ELSE g.away_score END   AS team_score,
                                             CASE
                                                 WHEN p.team_id = g.home_team_id THEN g.away_score
                                                 ELSE g.home_score END   AS opponent_score
                                          FROM game g
                                                   INNER JOIN player_teams p ON p.team_id IN (g.home_team_id, g.away_team_id)),
                     player_points AS (SELECT * FROM points WHERE @PlayerId IN (scorer_id, assist1_player_id, assist2_player_id))
                SELECT g.id AS game_id,
                       g.tournament_id,
                       g.date,
                       g.location,
                       g.rink,
                       g.type,
                       g.team_score,
                       g.opponent_score,
                       g.team_id,
                       g.opponent_team_id,
                       SUM(CASE WHEN @PlayerId = p.scorer_id THEN 1 ELSE 0 END)                                 AS goals,
                       SUM(CASE WHEN @PlayerId IN (p.assist1_player_id, p.assist2_player_id) THEN 1 ELSE 0 END) AS assists
                    FROM player_games g
                             LEFT OUTER JOIN player_points p ON g.id = p.game_id
                    GROUP BY g.id, g.tournament_id, g.date, g.location, g.rink, g.type, g.team_score, g.opponent_score, g.team_id,
                             g.opponent_team_id";

        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<PlayerGameSummary>(sql, new { PlayerId = id });
    }

    public async Task<IEnumerable<GoalieGameSummary>> GetGoalieGameByGame(int id)
    {
        string cacheKey = $"goalie_gbg_{id}";

        string sql = @"WITH
                        goalie_data AS (
                            SELECT * from players
                        ),
                        goalie_games_played AS (
                            SELECT P.*, G.id AS game_id, G.home_team_id, G.away_team_id, G.date
                                FROM goalie_data P
                                         RIGHT OUTER JOIN game G ON P.team_id IN (G.home_team_id, G.away_team_id)
                        ),
                        goalie_game_scores AS (
                            SELECT GP.game_id, GP.id,  GP.team_id, GP.date,
                                   CASE WHEN GP.team_id != GP.home_team_id THEN GP.home_team_id ELSE GP.away_team_id END AS opponent_team_id,
                                   SUM(
                                           CASE
                                               WHEN ( GP.team_id = GP.home_team_id AND is_hometeam = TRUE ) OR ( GP.team_id = GP.away_team_id AND is_hometeam = FALSE ) THEN 1
                                               ELSE 0
                                               END
                                   ) AS goals_for,
                                   SUM(
                                           CASE
                                               WHEN ( GP.team_id = GP.home_team_id AND is_hometeam = FALSE ) OR ( GP.team_id = GP.away_team_id AND is_hometeam = TRUE ) THEN 1
                                               ELSE 0
                                               END
                                   ) AS goals_against
                                FROM
                                    (
                                        SELECT *
                                            FROM goalie_games_played GP
                                    ) GP
                                        LEFT JOIN LATERAL (
                                        SELECT *
                                            FROM points P
                                            WHERE P.game_id = GP.game_id
                                        ) game_scores ON TRUE
                                GROUP BY GP.game_id, GP.id, opponent_team_id, GP.team_id, GP.date
                        )
                    SELECT
                        G.game_id AS GameId,
                        G.team_id AS TeamId,
                        G.opponent_team_id AS OpponentTeamId,
                        G.goals_for AS GoalsFor,
                        G.goals_against AS GoalsAgainst,
                        G.date AS GameDate
                        FROM goalie_game_scores G
                        WHERE G.id = @PlayerId";

        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<GoalieGameSummary>(sql, new { PlayerId = id });
    }

    public async Task<IEnumerable<GameGoal>> GetGameGoalsByGameId(int id)
    {
        string cacheKey = $"game_goals_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT p.game_id           AS gameid,
                                   p.scorer_id         AS scorerid,
                                   p.assist1_player_id AS assist1id,
                                   p.assist2_player_id AS assist2id,
                                   g0.name             AS scorername,
                                   a1.name             AS assist1name,
                                   a2.name             AS assist2name,
                                   p.player_jerseynum  AS scorerjersey,
                                   p.assist1_jerseynum AS assist1jersey,
                                   p.assist2_jerseynum AS assist2jersey,
                                   p.time,
                                   p.period,
                                   CASE WHEN p.is_hometeam THEN g.home_team_id ELSE g.away_team_id END AS teamid,
                                   a.uri               AS scorerprofilepic,
                                   t.name              AS teamname,
                                   t.logo_url          AS teamlogo
                                FROM points p
                                         JOIN game g ON p.game_id = g.id
                                         JOIN players g0 ON g0.id = p.scorer_id
                                         JOIN profile_pictures a ON a.player_id = g0.id
                                         JOIN team t ON t.id = (CASE WHEN p.is_hometeam THEN g.home_team_id ELSE g.away_team_id END)
                                         LEFT OUTER JOIN players a1 ON a1.id = p.assist1_player_id
                                         LEFT OUTER JOIN players a2 ON a2.id = p.assist2_player_id
                                WHERE p.game_id = @GameId
                                ORDER BY p.period, time DESC";

            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<GameGoal>(sql, new { GameId = id });
        }, cacheDuration);
    }

    public async Task<IEnumerable<GamePenalty>> GetGamePenaltiesByGameId(int id)
    {
        string cacheKey = $"game_penalties_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT x.game_id          AS gameid,
                                   x.player_jerseynum AS playerjersey,
                                   x.time,
                                   x.period,
                                   t.id               AS teamid,
                                   x.infraction_name  AS infraction,
                                   x.player_id        AS playerid,
                                   p.name             AS playername,
                                   t.logo_url         AS teamlogo
                                FROM penalties x
                                         JOIN players p ON p.id = x.player_id
                                         JOIN game g ON g.id = x.game_id
                                         JOIN team t ON t.id = (CASE WHEN x.is_hometeam THEN g.home_team_id ELSE g.away_team_id END)
                                WHERE x.game_id = @GameId
                                ORDER BY period, time DESC";

            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<GamePenalty>(sql, new { GameId = id });
        }, cacheDuration);
    }

    public async Task<IEnumerable<PlayerStatLine>> GetPlayerStats()
    {
        string cacheKey = $"player_stats";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"WITH
                            player_data AS (
                                SELECT *
                                    FROM players WHERE position != 'Goalie'
                            ),
                            points_with_teams AS (
                                SELECT G.id AS game_id, P.player_jerseynum, P.assist1_jerseynum, P.assist2_jerseynum, P.tournament_id,
                                       CASE
                                           WHEN is_hometeam = TRUE THEN home_team_id
                                           ELSE away_team_id
                                           END AS team_id
                                    FROM points P
                                             LEFT OUTER JOIN game G ON G.id = P.game_id
                            ),
                            points_with_players AS (
                                SELECT
                                    P.*, D1.id AS scorer_id, D2.id AS assist1_id, D3.id AS assist2_id
                                    FROM points_with_teams P
                                             LEFT OUTER JOIN player_data D1 ON D1.team_id = P.team_id
                                        AND D1.jersey_number = P.player_jerseynum
                                             LEFT OUTER JOIN player_data D2 ON D2.team_id = P.team_id
                                        AND D2.jersey_number = P.assist1_jerseynum
                                             LEFT OUTER JOIN player_data D3 ON D3.team_id = P.team_id
                                        AND D3.jersey_number = P.assist2_jerseynum
                            )
                        SELECT
                            id AS PlayerId,
                            name AS Name,
                            jersey_number AS PlayerNumber,
                            position AS Position,
                            team_id AS TeamId,
                            goals AS Goals,
                            assists AS Assists,
                            p.tournament_id AS TournamentId
                            FROM
                                (
                                    SELECT *
                                        FROM player_data
                                ) p
                                    LEFT JOIN LATERAL (
                                    SELECT
                                        SUM(
                                                CASE
                                                    WHEN p.id = scorer_id THEN 1
                                                    ELSE 0
                                                    END
                                        ) AS Goals,
                                        SUM(
                                                CASE
                                                    WHEN p.id IN (assist1_id, assist2_id) THEN 1
                                                    ELSE 0
                                                    END
                                        ) AS Assists,
                                        tournament_id
                                        FROM points_with_players
                                        GROUP BY tournament_id
                                    ) g ON TRUE
                            ORDER BY goals + assists DESC";

            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<PlayerStatLine>(sql);
        }, cacheDuration);
    }
    public async Task<IEnumerable<GoalieStatLine>> GetGoalieStats()
    {
        string cacheKey = $"goalie_stats";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"WITH
                            goalie_data AS (
                                SELECT * FROM players where position = 'Goalie'
                            ),
                            goalie_games_played AS (
                                SELECT P.*, G.id AS game_id, G.home_team_id, G.away_team_id, G.date
                                    FROM goalie_data P
                                             RIGHT OUTER JOIN game G ON P.team_id IN (G.home_team_id, G.away_team_id)
                            ),
                            goalie_game_scores AS (
                                SELECT GP.game_id, GP.id, GP.name, GP.jersey_number, GP.team_id, GP.date, GP.tournament_id,
                                       CASE WHEN GP.team_id != GP.home_team_id THEN GP.home_team_id ELSE GP.away_team_id END AS opponent_team_id,
                                       SUM(
                                               CASE
                                                   WHEN (
                                                            GP.team_id = GP.home_team_id
                                                                AND is_hometeam = TRUE
                                                            )
                                                       OR (
                                                            GP.team_id = GP.away_team_id
                                                                AND is_hometeam = FALSE
                                                            ) THEN 1
                                                   ELSE 0
                                                   END
                                       ) AS goals_for,
                                       SUM(
                                               CASE
                                                   WHEN ( GP.team_id = GP.home_team_id AND is_hometeam = FALSE ) OR ( GP.team_id = GP.away_team_id AND is_hometeam = TRUE ) THEN 1
                                                   ELSE 0
                                                   END
                                       ) AS goals_against
                                    FROM
                                        goalie_games_played GP
                                            LEFT JOIN LATERAL (
                                            SELECT *
                                                FROM points P
                                                WHERE P.game_id = GP.game_id
                                            ) game_scores ON TRUE
                                    GROUP BY GP.game_id, GP.id, GP.name, GP.jersey_number, opponent_team_id, GP.team_id, GP.date, GP.tournament_id
                            )
                        SELECT
                            G.id AS PlayerId,
                            G.name AS Name,
                            G.jersey_number AS PlayerNumber,
                            G.team_id AS TeamId,
                            G.tournament_id AS TournamentId,
                            AVG(G.goals_against) AS GAA,
                            SUM(CASE WHEN G.goals_against = 0 THEN 1 END) AS Shutouts
                            FROM goalie_game_scores G
                            GROUP BY G.id, G.name, G.jersey_number, G.team_id, G.tournament_id";

            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<GoalieStatLine>(sql);
        }, cacheDuration);
    }

    public async Task<IEnumerable<PlayerProfilePicture>> GetPlayerProfilePictures()
    {
        string cacheKey = $"player_profile_pics";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT
                            player_id AS PlayerId,
                            uri AS Source
                            FROM profile_pictures;";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<PlayerProfilePicture>(sql);
        }, cacheDuration);
    }

    public async Task<PlayerProfilePicture> GetPlayerProfilePictureById(int id)
    {
        string cacheKey = $"player_profile_pic_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT
                            player_id AS PlayerId,
                            uri AS Source
                            FROM profile_pictures
                            WHERE player_id = @PlayerId";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<PlayerProfilePicture>(sql, new { PlayerId = id }) ?? new();
        }, cacheDuration);
    }

    public async Task<string> SubmitRegistration(RegisterFormModel form)
    {
        form.FirstName = form.FirstName.ToLower();
        form.LastName = form.LastName.ToLower();
        form.Email = form.Email.ToLower();
        
        string sql = @"INSERT INTO
                          registration (firstname, lastname, email, birthday, position, highestlevel)
                        VALUES
                          (@FirstName, @LastName, @Email, @Birthday, @Position, @HighestLevel)";

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, form);

            return rowsAffected == 0 ? "Something went wrong" : string.Empty;
        }
        catch (PostgresException ex)
        {
            if (ex.ConstraintName == "registration_email_key")
            {
                return "That email is already registered.";
            }

            return "Something went wrong...";
        }
        catch (Exception)
        {
            return "Something went wrong...";
        }
    }

    public async Task<IEnumerable<RegisterFormModel>> GetRegistrationsAsync()
    {
        string sql = @"SELECT
                          *
                        FROM
                          registration";
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<RegisterFormModel>(sql);
    }

    public async Task<RegisterFormModel?> GetRegistrationByEmailAsync(string email)
    {
        string sql = @"SELECT *
                        FROM registration
                        WHERE email = @Email";

        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<RegisterFormModel>(sql, new { Email = email });
    }

    public async Task<string> AdmitUserAsync(RegisterFormModel form)
    {
        // check if user exists: if yes then do not admit
        string checkSql = @"SELECT * FROM account WHERE email = @Email";
        await using var connection = new NpgsqlConnection(connectionString);
        var users = await connection.QueryAsync<RegisterFormModel>(checkSql, new { Email = form.Email });
        if (users.Count() > 0)
        {
            return "User already admitted";
        }
        
        string sql = @"INSERT INTO
                          account (firstname, lastname, email, birthday, position, highestlevel)
                        VALUES (@FirstName, @LastName, @Email, @Birthday, @Position, @HighestLevel)";
        
        var rowsAffected = await connection.ExecuteAsync(sql, form);

        return rowsAffected == 0 ? "Something went wrong" : string.Empty;
    }

    public async Task<IEnumerable<BCAccount>> GetAccountsAsync()
    {
        string sql = @"SELECT
                          *
                        FROM
                          account";
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<BCAccount>(sql);
    }

    public async Task<BCAccount?> GetAccountByEmailAsync(string email)
    {
        string sql = @"SELECT
                          *
                        FROM
                          account
                        WHERE
                          email = @Email";
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<BCAccount>(sql, new { Email = email });
    }

    public async Task UpdateAccountProfilePictureAsync(string email, string imagePath)
    {
        string sql = @"UPDATE account
                        SET
                          profilepicture = @ImagePath
                        WHERE
                          email = @Email";
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.ExecuteAsync(sql, new { Email = email, ImagePath = imagePath });
        
        cacheService.Clear("player_profile_pics");
    }

    public async Task<IEnumerable<BCTournament>> GetTournamentsAsync()
    {
        string sql = @"SELECT *
                        FROM tournament";
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<BCTournament>(sql);
    }    
    
    public async Task<BCTournament?> GetTournamentByYearAsync(string year)
    {
        string sql = @"SELECT *
                        FROM tournament
                        WHERE EXTRACT(YEAR FROM start_date) = @Year";
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<BCTournament>(sql, new { Year = year });
    }

    public async Task SetUserAsPayedAsync(string email)
    {
        string sql = @"UPDATE registration
                        SET payed = true
                        WHERE email = @Email";
        
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.ExecuteAsync(sql, new { Email = email });
    }

    public async Task<BCDraftPick?> GetMostRecentDraftPickAsync(int draftId)
    {
        string sql = @"SELECT *
                        FROM draftpick
                        WHERE round = (SELECT MAX(round)
                                           FROM draftpick
                                           WHERE draft_id = @DraftId)
                          AND pick = (SELECT MAX(pick)
                                          FROM draftpick
                                          WHERE draft_id = @DraftId)
                          AND draft_id = @DraftId";
        
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QuerySingleOrDefaultAsync<BCDraftPick>(sql, new { DraftId = draftId });
    }

    public async Task<BCTeam?> GetTeamByDraftOrderAsync(int draftId, int order)
    {
        string sql = @"select
                          t.*
                        from
                          team t
                          inner join draftorder o on t.id = o.team_id
                        where
                          o.order = @Order
                          and o.draft_id = @DraftId";
        
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QuerySingleOrDefaultAsync<BCTeam>(sql, new { DraftId = draftId, Order = order });
    }

    public async Task<IEnumerable<BCTeam>> GetTeamsInTournamentAsync(int tournamentId)
    {
        string sql = @"SELECT *
                        FROM team
                        WHERE tournament_id = @TournamentId";
        
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<BCTeam>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<BCDraftOrder>> GetDraftOrderAsync(int draftId)
    {
        string sql = @"SELECT *
                        FROM draftorder
                        WHERE draft_id = @DraftId";
        
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<BCDraftOrder>(sql, new { DraftId = draftId });
    }

    public async Task DraftPlayerAsync(PlayerProfile player, BCTeam team, BCDraftPick draftPick)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        
        string playerSql = @"UPDATE players
                        SET team_id = @TeamId
                            WHERE id = @PlayerId
                            AND tournament_id = @TournamentId";
        await connection.ExecuteAsync(playerSql, new { TeamId = team.id, PlayerId = player.id, TournamentId = player.tournament_id });
        
        draftPick.player_id = player.id;
        string draftSql = @"INSERT INTO draftpick (draft_id, round, pick, player_id)
                                VALUES (@draft_id, @round, @pick, @player_id)";
        await connection.ExecuteAsync(draftSql, draftPick);
    }

    public async Task<IEnumerable<BCDraftPickDetail>> GetDraftPicksAsync(int draftId)
    {
        string sql = @"SELECT dp.*,
                               p.name,
                               p.dob        AS birthday,
                               p.team_id    AS teamid,
                               p.position,
                               a.profilepicture,
                               t.name       AS teamname,
                               t.name_short AS teamnameshort,
                               t.id         AS teamid,
                               t.logo_url   AS teamlogo,
                               t.primary_color_hex AS PrimaryColorHex,
                               t.secondary_color_hex AS SecondaryColorHex,
                               t.tertiary_color_hex AS TertiaryColorHex
                            FROM draftpick dp
                                     INNER JOIN players p ON p.id = dp.player_id AND dp.draft_id = @DraftId
                                     INNER JOIN team t ON t.id = p.team_id
                                     LEFT OUTER JOIN account a ON a.id = p.account_id
                            ORDER BY round, pick";
        
        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<BCDraftPickDetail>(sql, new { DraftId = draftId });
    }

    public async Task ResetDraftAsync(int draftId)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        
        // delete all draft picks in draft
        string draftSql = @"DELETE FROM draftpick
                            WHERE draft_id = @DraftId";
        await connection.ExecuteAsync(draftSql, new { DraftId = draftId });
        
        // set all player teams to null
        string playerSql = @"UPDATE players
                                SET team_id = NULL
                                    WHERE tournament_id = (SELECT tournament_id FROM draft WHERE id = @DraftId)";
        await connection.ExecuteAsync(playerSql, new { DraftId = draftId });
    }

}


