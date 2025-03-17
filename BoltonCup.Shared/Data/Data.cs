namespace BoltonCup.Shared.Data;
using Dapper;
using Npgsql;

public interface IBCData
{
    Task<IEnumerable<Team>> GetTeams();
    Task<Team?> GetTeamById(int id);
    Task<IEnumerable<ScheduledGame>> GetSchedule();
    Task<ScheduledGame?> GetGameById(int id);
    Task<IEnumerable<TeamPlayer>?> GetRosterByTeamId(int teamId);
    Task<Player?> GetPlayerById(int playerId);
    Task<PlayerProfile?> GetPlayerProfileById(int playerId);
    Task<IEnumerable<PlayerGameSummary>> GetPlayerGameByGame(int playerId);
    Task<IEnumerable<GoalieGameSummary>> GetGoalieGameByGame(int goalieId);
    Task<IEnumerable<GameGoal>> GetGameGoalsByGameId(int gameId);
    Task<IEnumerable<GamePenalty>> GetGamePenaltiesByGameId(int gameId);
    Task<IEnumerable<PlayerStatline>> GetPlayerStats();
    Task<IEnumerable<GoalieStatline>> GetGoalieStats();
    Task<GameScore?> GetGameScoreById(int gameId);
    Task<IEnumerable<PlayerProfilePicture>> GetPlayerProfilePictures();
    Task<PlayerProfilePicture> GetPlayerProfilePictureById(int playerId);
    Task<string> SubmitRegistration(RegisterFormModel form);
    Task<IEnumerable<RegisterFormModel>> GetRegistrationsAsync();
    Task<string> AdmitUserAsync(RegisterFormModel form);
    Task<IEnumerable<BCAccount>> GetAccountsAsync();
    Task<BCAccount?> GetAccountByEmailAsync(string email);
    Task UpdateAccountProfilePictureAsync(string email, string imagePath);
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

    public async Task<IEnumerable<Team>> GetTeams()
    {
        string cacheKey = "teams";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT
                          T.id as Id,
                          T.name as Name,
                          T.name_short as ShortName,
                          T.primary_color_hex as PrimaryHex,
                          T.secondary_color_hex as SecondaryHex,
                          T.tertiary_color_hex as TertiaryHex,
                          T.logo_url as LogoUrl
                        FROM
                          teams T";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<Team>(sql);
        }, cacheDuration);
    }

    public async Task<Team?> GetTeamById(int id)
    {
        string cacheKey = $"team_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT
                            T.id AS Id,
                            T.name AS Name,
                            T.name_short as ShortName,
                            T.primary_color_hex AS PrimaryHex,
                            T.secondary_color_hex AS SecondaryHex,
                            T.tertiary_color_hex AS TertiaryHex,
                            T.logo_url as LogoUrl
                        FROM
                            teams T
                        WHERE
                            T.id = @TeamId";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Team>(sql, new { TeamId = id });
        }, cacheDuration);
    }

    public async Task<IEnumerable<ScheduledGame>> GetSchedule()
    {
        string cacheKey = "schedule";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {

            string sql = @"SELECT
                                G.id as GameId,
                                G.home_team_id as HomeTeamId,
                                G.away_team_id as AwayTeamId,
                                G.date as Date,
                                G.type as GameType,
                                G.location as Location,
                                G.rink as Rink
                            FROM
                                games G
                            ORDER BY G.date ASC";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<ScheduledGame>(sql);
        }, cacheDuration);
    }

    public async Task<ScheduledGame?> GetGameById(int id)
    {
        string cacheKey = $"game_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT
                            G.id as GameId,
                            G.home_team_id as HomeTeamId,
                            G.away_team_id as AwayTeamId,
                            G.date as Date,
                            G.type as GameType,
                            G.location as Location,
                            G.rink as Rink
                            FROM
                            games G
                            WHERE G.id = @GameId";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<ScheduledGame>(sql, new { GameId = id });
        }, cacheDuration);
    }

    public async Task<IEnumerable<TeamPlayer>?> GetRosterByTeamId(int id)
    {
        string cacheKey = $"roster_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT
                                P.name AS Name,
                                P.dob AS Birthday,
                                R.player_number AS JerseyNumber,
                                R.player_id AS PlayerId,
                                R.position AS Position,
                                R.team_id AS TeamId
                            FROM
                                rosters R
                            INNER JOIN players P ON R.player_id = P.id
                                AND R.team_id = @TeamId";

            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<TeamPlayer>(sql, new { TeamId = id });
        }, cacheDuration);
    }

    public async Task<Player?> GetPlayerById(int id)
    {
        string cacheKey = $"player_{id}";

        string sql = @"SELECT
                        P.id AS Id,
                        P.name AS Name,
                        P.dob AS Birthday,
                        P.preferred_beer AS PreferredBeer
                    FROM
                        players P
                    WHERE
                        P.id = @PlayerId";

        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<Player>(sql, new { PlayerId = id });
    }

    public async Task<PlayerProfile?> GetPlayerProfileById(int id)
    {
        string cacheKey = $"teamplayer_{id}";

        string sql = @"SELECT
                        P.id AS PlayerId,
                        P.name AS Name,
                        P.dob AS Birthday,
                        P.preferred_beer AS PreferredBeer,
                        R.team_id AS CurrentTeamId,
                        R.player_number AS JerseyNumber,
                        R.position AS Position,
                        CASE WHEN T.winning_team_id IS NOT NULL THEN true ELSE false END AS IsWinner
                    FROM
                        players P
                        JOIN rosters R ON P.id = R.player_id
                        LEFT OUTER JOIN tournaments T ON T.winning_team_id = R.team_id
                    WHERE P.id = @PlayerId";

        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<PlayerProfile>(sql, new { PlayerId = id });
    }

    public async Task<IEnumerable<PlayerGameSummary>> GetPlayerGameByGame(int id)
    {
        string cacheKey = $"player_gbg_{id}";

        string sql = @"WITH
                        current_team AS (
                            SELECT R.team_id, R.player_number
                            FROM rosters R
                            WHERE R.player_id = @PlayerId
                        ),
                        games_played AS (
                            SELECT G.id as game_id,
                                CASE
                                    WHEN C.team_id != G.home_team_id THEN G.home_team_id
                                    ELSE G.away_team_id
                                END AS opponent_team_id, G.date, G.type, G.location, G.rink, G.home_team_id, G.away_team_id, C.*
                            FROM games G
                            INNER JOIN current_team C on C.team_id IN (G.home_team_id, G.away_team_id)
                        ),
                        team_points AS (
                            SELECT GP.game_id, GP.player_number, GP.opponent_team_id, GP.team_id, GP.date,
                                COALESCE(P.player_jerseynum, -1) AS player_jerseynum,
                                COALESCE(P.assist1_jerseynum, -1) AS assist1_jerseynum,
                                COALESCE(P.assist2_jerseynum, -1) AS assist2_jerseynum
                            FROM points P
                            RIGHT OUTER JOIN games_played GP ON P.game_id = GP.game_id
                            AND ((GP.team_id = GP.home_team_id AND P.is_hometeam = TRUE) OR (GP.team_id = GP.away_team_id AND P.is_hometeam = FALSE))
                        )
                        SELECT
                        SUM(
                            CASE
                            WHEN player_number = player_jerseynum THEN 1
                            ELSE 0
                            END
                        ) AS goals,
                        SUM(
                            CASE
                            WHEN player_number IN (assist1_jerseynum, assist2_jerseynum) THEN 1
                            ELSE 0
                            END
                        ) AS assists,
                        game_id AS GameId,
                        opponent_team_id AS OpponentTeamId,
                        team_id AS TeamId,
                        date AS GameDate
                        FROM team_points
                        GROUP BY game_id, opponent_team_id, team_id, date";

        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<PlayerGameSummary>(sql, new { PlayerId = id });
    }

    public async Task<IEnumerable<GoalieGameSummary>> GetGoalieGameByGame(int id)
    {
        string cacheKey = $"goalie_gbg_{id}";

        string sql = @"WITH
                        goalie_data AS (
                            SELECT P.id AS player_id, P.name, R.player_number, R.position, R.team_id
                            FROM players P
                            INNER JOIN rosters R ON R.player_id = P.id AND R.position = 'Goalie'
                        ),
                        goalie_games_played AS (
                            SELECT P.*, G.id AS game_id, G.home_team_id, G.away_team_id, G.date
                            FROM goalie_data P
                            RIGHT OUTER JOIN games G ON P.team_id IN (G.home_team_id, G.away_team_id)
                        ),
                        goalie_game_scores AS (
                            SELECT GP.game_id, GP.player_id,  GP.team_id, GP.date,
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
                            GROUP BY GP.game_id, GP.player_id, opponent_team_id, GP.team_id, GP.date
                        )
                        SELECT
                        G.game_id AS GameId,
                        G.team_id AS TeamId,
                        G.opponent_team_id AS OpponentTeamId,
                        G.goals_for AS GoalsFor,
                        G.goals_against AS GoalsAgainst,
                        G.date AS GameDate
                        FROM goalie_game_scores G
                        WHERE G.player_id = @PlayerId";

        await using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryAsync<GoalieGameSummary>(sql, new { PlayerId = id });
    }

    public async Task<IEnumerable<GameGoal>> GetGameGoalsByGameId(int id)
    {
        string cacheKey = $"game_goals_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT
                            points.game_id AS GameId,
                            points.player_jerseynum AS ScorerJersey,
                            points.assist1_jerseynum AS Assist1Jersey,
                            points.assist2_jerseynum AS Assist2Jersey,
                            points.time AS Time,
                            points.period AS Period,
                            points.team_id AS TeamId,
                            r1.player_id AS ScorerId,
                            r2.player_id AS Assist1Id,
                            r3.player_id AS Assist2Id,
                            p1.name AS ScorerName,
                            p2.name AS Assist1Name,
                            p3.name AS Assist2Name
                            FROM
                            (
                                SELECT p.game_id, p.player_jerseynum, p.assist1_jerseynum, p.assist2_jerseynum, p.time, p.period,
                                CASE
                                    WHEN p.is_hometeam = true THEN home_team_id
                                    WHEN p.is_hometeam = false THEN away_team_id
                                END AS team_id
                                FROM points p
                                INNER JOIN games g ON g.id = p.game_id
                                WHERE g.id = @GameId
                                ORDER BY time desc
                            ) points
                            LEFT JOIN rosters r1 ON points.player_jerseynum = r1.player_number AND points.team_id = r1.team_id
                            LEFT JOIN rosters r2 ON points.assist1_jerseynum = r2.player_number AND points.team_id = r2.team_id
                            LEFT JOIN rosters r3 ON points.assist2_jerseynum = r3.player_number AND points.team_id = r3.team_id
                            LEFT JOIN players p1 ON r1.player_id = p1.id
                            LEFT JOIN players p2 ON r2.player_id = p2.id
                            LEFT JOIN players p3 ON r3.player_id = p3.id
                            ORDER BY points.period asc, points.time desc";

            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<GameGoal>(sql, new { GameId = id });
        }, cacheDuration);
    }

    public async Task<IEnumerable<GamePenalty>> GetGamePenaltiesByGameId(int id)
    {
        string cacheKey = $"game_penalties_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT
                            penalties.game_id AS GameId,
                            penalties.player_jerseynum AS PlayerJersey,
                            penalties.time AS Time,
                            penalties.period AS Period,
                            penalties.team_id AS TeamId,
                            penalties.infraction_name AS Infraction,
                            r1.player_id AS PlayerId,
                            p1.name AS PlayerName
                            FROM
                            (
                                SELECT p.game_id, p.player_jerseynum, p.time, p.period, p.infraction_name,
                                CASE
                                    WHEN p.is_hometeam = true THEN home_team_id
                                    WHEN p.is_hometeam = false THEN away_team_id
                                END AS team_id
                                FROM penalties p
                                INNER JOIN games g ON g.id = p.game_id
                                WHERE g.id = @GameId
                                ORDER BY time desc
                            ) penalties
                            LEFT JOIN rosters r1 ON penalties.player_jerseynum = r1.player_number AND penalties.team_id = r1.team_id
                            LEFT JOIN players p1 ON r1.player_id = p1.id
                            ORDER BY penalties.period asc, penalties.time desc";

            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<GamePenalty>(sql, new { GameId = id });
        }, cacheDuration);
    }

    public async Task<IEnumerable<PlayerStatline>> GetPlayerStats()
    {
        string cacheKey = $"player_stats";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"WITH
                            player_data AS (
                                SELECT P.id AS player_id, P.name, R.player_number, R.position, R.team_id
                                FROM players P
                                INNER JOIN rosters R ON R.player_id = P.id
                            ),
                            points_with_teams AS (
                                SELECT G.id AS game_id, P.player_jerseynum, P.assist1_jerseynum, P.assist2_jerseynum,
                                CASE
                                    WHEN is_hometeam = TRUE THEN home_team_id
                                    ELSE away_team_id
                                END AS team_id
                                FROM points P
                                LEFT OUTER JOIN games G ON G.id = P.game_id
                            ),
                            points_with_players AS (
                                SELECT
                                P.*, D1.player_id AS scorer_id, D2.player_id AS assist1_id, D3.player_id AS assist2_id
                                FROM points_with_teams P
                                LEFT OUTER JOIN player_data D1 ON D1.team_id = P.team_id
                                AND D1.player_number = P.player_jerseynum
                                LEFT OUTER JOIN player_data D2 ON D2.team_id = P.team_id
                                AND D2.player_number = P.assist1_jerseynum
                                LEFT OUTER JOIN player_data D3 ON D3.team_id = P.team_id
                                AND D3.player_number = P.assist2_jerseynum
                            )
                            SELECT
                            player_id AS PlayerId,
                            name AS Name,
                            player_number AS PlayerNumber,
                            position AS Position,
                            team_id AS TeamId,
                            goals AS Goals,
                            assists AS Assists
                            FROM
                            (
                                SELECT *
                                FROM player_data
                            ) p
                            LEFT JOIN LATERAL (
                                SELECT
                                SUM(
                                    CASE
                                    WHEN p.player_id = scorer_id THEN 1
                                    ELSE 0
                                    END
                                ) AS Goals,
                                SUM(
                                    CASE
                                    WHEN p.player_id IN (assist1_id, assist2_id) THEN 1
                                    ELSE 0
                                    END
                                ) AS Assists
                                FROM points_with_players
                            ) g ON TRUE
                            ORDER BY goals + assists DESC";

            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<PlayerStatline>(sql);
        }, cacheDuration);
    }
    public async Task<IEnumerable<GoalieStatline>> GetGoalieStats()
    {
        string cacheKey = $"goalie_stats";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"WITH
                            goalie_data AS (
                                SELECT P.id AS player_id, P.name, R.player_number, R.position, R.team_id
                                FROM players P
                                INNER JOIN rosters R ON R.player_id = P.id AND R.position = 'Goalie'
                            ),
                            goalie_games_played AS (
                                SELECT P.*, G.id AS game_id, G.home_team_id, G.away_team_id, G.date
                                FROM goalie_data P
                                RIGHT OUTER JOIN games G ON P.team_id IN (G.home_team_id, G.away_team_id)
                            ),
                            goalie_game_scores AS (
                                SELECT GP.game_id, GP.player_id,  GP.name, GP.player_number, GP.team_id, GP.date,
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
                                (
                                    SELECT *
                                    FROM goalie_games_played GP
                                ) GP
                                LEFT JOIN LATERAL (
                                    SELECT *
                                    FROM points P
                                    WHERE P.game_id = GP.game_id
                                ) game_scores ON TRUE
                                GROUP BY GP.game_id, GP.player_id, GP.name, GP.player_number, opponent_team_id, GP.team_id, GP.date
                            )
                            SELECT
                            G.player_id AS PlayerId,
                            G.name AS Name,
                            G.player_number AS PlayerNumber,
                            G.team_id AS TeamId,
                            AVG(G.goals_against) AS GAA,
                            SUM(CASE WHEN G.goals_against = 0 THEN 1 END) AS Shutouts
                            FROM goalie_game_scores G
                            GROUP BY G.player_id, G.name, G.player_number, G.team_id";

            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryAsync<GoalieStatline>(sql);
        }, cacheDuration);
    }

    public async Task<GameScore?> GetGameScoreById(int id)
    {
        string cacheKey = $"game_score_{id}";

        return await cacheService.GetOrAddAsync(cacheKey, async () =>
        {
            string sql = @"SELECT
                            P.game_id AS GameId,
                            G.home_team_id AS HomeTeamId,
                            G.away_team_id AS AwayTeamId,
                            SUM(
                                CASE
                                WHEN is_hometeam = TRUE THEN 1
                                END
                            ) AS HomeScore,
                            SUM(
                                CASE
                                WHEN is_hometeam = FALSE THEN 1
                                END
                            ) AS AwayScore
                            FROM points P
                            JOIN games G ON G.id = P.game_id
                            WHERE P.game_id = @GameId
                            GROUP BY game_id, G.home_team_id, G.away_team_id";
            await using var connection = new NpgsqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<GameScore>(sql, new { GameId = id });
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

        await using var connection = new NpgsqlConnection(connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, form);

        return rowsAffected == 0 ? "Something went wrong" : string.Empty;
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
    }
}


