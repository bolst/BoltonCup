namespace BoltonCup.Shared.Data;
using Dapper;
using Npgsql;

public interface IBCData
{
    Task<IEnumerable<BCTeam>> GetTeams();
    Task<BCTeam?> GetTeamById(int id);
    Task<IEnumerable<BCGame>> GetSchedule();
    Task<IEnumerable<BCGame>> GetPlayerSchedule(int playerId);
    Task<BCGame?> GetGameById(int id);
    Task<IEnumerable<PlayerProfile>> GetRosterByTeamId(int teamId);
    Task<IEnumerable<PlayerProfile>> GetAllTournamentPlayersAsync(int tournamentId);
    Task<PlayerProfile?> GetPlayerProfileById(int playerId);
    Task<PlayerProfile?> GetUserTournamentPlayerProfileAsync(int accountId, int tournamentId);
    Task<IEnumerable<PlayerGameSummary>> GetPlayerGameByGame(int accountId, int? tournamentId = null);
    Task<IEnumerable<GoalieGameSummary>> GetGoalieGameByGame(int accountId, int? tournamentId = null);
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
    Task<string> RemoveAdmittedUserAsync(BCAccount account);
    Task<IEnumerable<BCAccount>> GetAccountsAsync();
    Task<BCAccount?> GetAccountByEmailAsync(string email);
    Task<BCAccount?> GetAccountByIdAsync(int accountId);
    Task UpdateAccountProfilePictureAsync(string email, string imagePath);
    Task<IEnumerable<BCTournament>> GetTournamentsAsync();
    Task<BCTournament?> GetTournamentByYearAsync(string year);
    Task<BCTournament?> GetCurrentTournamentAsync();
    Task SetUserAsPayedAsync(string email);
    Task ConfigPlayerProfileAsync(RegisterFormModel form, int tournamentId);
    Task<BCDraftPick?> GetMostRecentDraftPickAsync(int draftId);
    Task<BCTeam?> GetTeamByDraftOrderAsync(int draftId, int order);
    Task<IEnumerable<BCTeam>> GetTeamsInTournamentAsync(int tournamentId);
    Task<IEnumerable<BCDraftOrder>> GetDraftOrderAsync(int draftId);
    Task DraftPlayerAsync(PlayerProfile player, BCTeam team, BCDraftPick draftPick);
    Task<IEnumerable<BCDraftPickDetail>> GetDraftPicksAsync(int draftId);
    Task ResetDraftAsync(int draftId);
    Task<IEnumerable<BCSponsor>> GetTournamentSponsorsAsync(int tournamentId);
}

public class BCData : DapperBase, IBCData
{
    private readonly string connectionString;
    private readonly ICacheService cacheService;
    private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);


    public BCData(string _connectionString, ICacheService _cacheService) : base(_connectionString)
    {
        connectionString = _connectionString;
        cacheService = _cacheService;
    }

    public async Task<IEnumerable<BCTeam>> GetTeams()
    {
        string sql = @"SELECT *
                    FROM
                      team T";
        return await QueryDbAsync<BCTeam>(sql);
    }

    public async Task<BCTeam?> GetTeamById(int id)
    {
        string sql = @"SELECT *
                    FROM
                        team T
                    WHERE
                        T.id = @TeamId";
        return await QueryDbSingleAsync<BCTeam>(sql, new { TeamId = id });
    }

    public async Task<IEnumerable<BCGame>> GetSchedule()
    {
        string sql = @"SELECT g.*,
                           h.name       AS hometeamname,
                           h.name_short AS hometeamnameshort,
                           h.logo_url   AS hometeamlogo,
                           a.name       AS awayteamname,
                           a.name_short AS awayteamnameshort,
                           a.logo_url   AS awayteamlogo
                        FROM game g
                                 LEFT OUTER JOIN team h ON g.home_team_id = h.id
                                 LEFT OUTER JOIN team a ON g.away_team_id = a.id
                        ORDER BY g.date ASC";
        return await QueryDbAsync<BCGame>(sql);
    }

    public async Task<IEnumerable<BCGame>> GetPlayerSchedule(int playerId)
    {
        string sql = @"SELECT g.*,
                           h.name       AS hometeamname,
                           h.name_short AS hometeamnameshort,
                           h.logo_url   AS hometeamlogo,
                           a.name       AS awayteamname,
                           a.name_short AS awayteamnameshort,
                           a.logo_url   AS awayteamlogo
                        FROM game g
                                 LEFT OUTER JOIN team h ON g.home_team_id = h.id
                                 LEFT OUTER JOIN team a ON g.away_team_id = a.id
                                 INNER JOIN players p ON p.team_id IN (g.home_team_id, g.away_team_id) AND p.id = @PlayerId
                        ORDER BY g.date ASC";
        return await QueryDbAsync<BCGame>(sql, new { PlayerId = playerId });
    }

    public async Task<BCGame?> GetGameById(int id)
    {
        string sql = @"SELECT g.*,
                           h.name       AS hometeamname,
                           h.name_short AS hometeamnameshort,
                           h.logo_url   AS hometeamlogo,
                           a.name       AS awayteamname,
                           a.name_short AS awayteamnameshort,
                           a.logo_url   AS awayteamlogo
                        FROM game g
                                 LEFT OUTER JOIN team h ON g.home_team_id = h.id
                                 LEFT OUTER JOIN team a ON g.away_team_id = a.id
                        WHERE g.id = @GameId
                        ORDER BY g.date ASC";
        return await QueryDbSingleAsync<BCGame>(sql, new { GameId = id });
    }

    public async Task<IEnumerable<PlayerProfile>> GetRosterByTeamId(int id)
    {
        string sql = @"SELECT *, p.id
                        FROM players p
                            LEFT OUTER JOIN account a ON p.account_id = a.id AND a.isactive = TRUE
                        WHERE p.team_id = @TeamId";

        return await QueryDbAsync<PlayerProfile>(sql, new { TeamId = id });
    }

    public async Task<IEnumerable<PlayerProfile>> GetAllTournamentPlayersAsync(int tournamentId)
    {
        string sql = @"SELECT *, p.id
                        FROM players p
                                 LEFT OUTER JOIN account a ON p.account_id = a.id AND a.isactive = TRUE
                        WHERE tournament_id = @TournamentId";
        
        return await QueryDbAsync<PlayerProfile>(sql, new { TournamentId = tournamentId });
    }
    
    public async Task<PlayerProfile?> GetPlayerProfileById(int id)
    {
        string sql = @"SELECT *, p.id
                        FROM players p 
                            LEFT OUTER JOIN account a ON p.account_id = a.id AND a.isactive = TRUE
                        WHERE p.id = @PlayerId";

        return await QueryDbSingleAsync<PlayerProfile>(sql, new { PlayerId = id });
    }

    public async Task<PlayerProfile?> GetUserTournamentPlayerProfileAsync(int accountId, int tournamentId)
    {
        string sql = @"SELECT *
                        FROM players
                        WHERE account_id = @AccountId
                          AND tournament_id = @TournamentId";
        return await QueryDbSingleAsync<PlayerProfile>(sql,
            new { AccountId = accountId, TournamentId = tournamentId });
    }

    public async Task<IEnumerable<PlayerGameSummary>> GetPlayerGameByGame(int accountId, int? tournamentId = null)
    {
        string sql = @"with
                        player_teams as (
                            select
                                t.*,
                                p.id as player_id
                                from
                                    team t
                                        inner join players p on p.team_id = t.id
                                        and p.account_id = @AccountId
                        ),
                        player_games as (
                            select
                                g.*,
                                t.id as team_id,
                                t.name as team_name,
                                t.name_short as team_name_short,
                                t.logo_url as team_logo_url,
                                o.id  as opponent_team_id,
                                o.name as opponent_name,
                                o.name_short as opponent_name_short,
                                o.logo_url as opponent_logo_url,
                                case when t.id = home_team_id then home_score else away_score end as team_score,
                                case when t.id != home_team_id then home_score else away_score end as opponent_team_score,
                                t.player_id,
                                t.name
                                from
                                    game g
                                        inner join player_teams t on t.id in (g.home_team_id, g.away_team_id)
                                        inner join team o on o.id = case when t.id = home_team_id then away_team_id else home_team_id end
                                        and g.date < now()
                                        {0}
                        )
                    select
                        *,
                        get_player_game_goals (@AccountId, id) as goals,
                        get_player_game_assists (@AccountId, id) as assists
                        from
                            player_games
                        order by date";

        sql = string.Format(sql, tournamentId is not null ? "and g.tournament_id = @TournamentId" : string.Empty);

        return await QueryDbAsync<PlayerGameSummary>(sql, new { AccountId = accountId, TournamentId = tournamentId });
    }

    public async Task<IEnumerable<GoalieGameSummary>> GetGoalieGameByGame(int accountId, int? tournamentId = null)
    {
        string sql = @"with
                        player_teams as (
                            select
                                t.*,
                                p.id as player_id
                                from
                                    team t
                                        inner join players p on p.team_id = t.id
                                        and p.account_id = @AccountId
                        ),
                        player_games as (
                            select
                                g.*,
                                t.id as team_id,
                                t.name as team_name,
                                t.name_short as team_name_short,
                                t.logo_url as team_logo_url,
                                o.id  as opponent_team_id,
                                o.name as opponent_name,
                                o.name_short as opponent_name_short,
                                o.logo_url as opponent_logo_url,
                                case when t.id = home_team_id then home_score else away_score end as team_score,
                                case when t.id != home_team_id then home_score else away_score end as opponent_team_score,
                                t.player_id,
                                t.name
                                from
                                    game g
                                        inner join player_teams t on t.id in (g.home_team_id, g.away_team_id)
                                        inner join team o on o.id = case when t.id = home_team_id then away_team_id else home_team_id end
                                        and g.date < now()
                                        {0}
                        )
                    select
                        *,
                        case when team_score > opponent_team_score then true else false end as win
                        from
                            player_games
                        order by date";
        
        sql = string.Format(sql, tournamentId is not null ? "and g.tournament_id = @TournamentId" : string.Empty);

        return await QueryDbAsync<GoalieGameSummary>(sql, new { AccountId = accountId, TournamentId = tournamentId });
    }

    public async Task<IEnumerable<GameGoal>> GetGameGoalsByGameId(int id)
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

        return await QueryDbAsync<GameGoal>(sql, new { GameId = id });
    }

    public async Task<IEnumerable<GamePenalty>> GetGamePenaltiesByGameId(int id)
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

        return await QueryDbAsync<GamePenalty>(sql, new { GameId = id });
    }

    public async Task<IEnumerable<PlayerStatLine>> GetPlayerStats()
    {
        string sql = @"WITH
                        player_data AS (
                            SELECT *
                                FROM players WHERE position != 'goalie'
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
        return await QueryDbAsync<PlayerStatLine>(sql);
    }
    public async Task<IEnumerable<GoalieStatLine>> GetGoalieStats()
    {
        string sql = @"WITH
                        goalie_data AS (
                            SELECT * FROM players where position = 'goalie'
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

        return await QueryDbAsync<GoalieStatLine>(sql);
    }

    public async Task<IEnumerable<PlayerProfilePicture>> GetPlayerProfilePictures()
    {
        string sql = @"SELECT
                        player_id AS PlayerId,
                        uri AS Source
                        FROM profile_pictures;";
        return await QueryDbAsync<PlayerProfilePicture>(sql);
    }

    public async Task<PlayerProfilePicture> GetPlayerProfilePictureById(int id)
    {
        string sql = @"SELECT
                        player_id AS PlayerId,
                        uri AS Source
                        FROM profile_pictures
                        WHERE player_id = @PlayerId";
        return await QueryDbSingleAsync<PlayerProfilePicture>(sql, new { PlayerId = id }) ?? new();
    }

    public async Task<string> SubmitRegistration(RegisterFormModel form)
    {
        form.FirstName = form.FirstName.ToLower();
        form.LastName = form.LastName.ToLower();
        form.Email = form.Email.ToLower();
        
        string sql = @"INSERT INTO
                          registration (firstname, lastname, email, birthday, position, highestlevel, jerseysize)
                        VALUES
                          (@FirstName, @LastName, @Email, @Birthday, @Position, @HighestLevel, @JerseySize);";

        try
        {
            var rowsAffected = await ExecuteSqlAsync(sql, form);

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
        return await QueryDbAsync<RegisterFormModel>(sql);
    }

    public async Task<RegisterFormModel?> GetRegistrationByEmailAsync(string email)
    {
        string sql = @"SELECT *
                        FROM registration
                        WHERE email = @Email";

        return await QueryDbSingleAsync<RegisterFormModel>(sql, new { Email = email });
    }

    public async Task<string> AdmitUserAsync(RegisterFormModel form)
    {
        // check if user exists: if yes then set them to active
        string checkSql = @"SELECT * FROM account WHERE email = @Email";
        var users = await QueryDbAsync<RegisterFormModel>(checkSql, new { Email = form.Email });
        if (users.Any())
        {
            string activeSql = @"UPDATE account
                                    SET isactive = TRUE
                                        WHERE email = @Email";
            var activeRowsAffected = await ExecuteSqlAsync(activeSql, form);
            return activeRowsAffected == 0 ? "Something went wrong" : string.Empty;
        }
        
        string sql = @"INSERT INTO
                          account (firstname, lastname, email, birthday, position, highestlevel)
                        VALUES (@FirstName, @LastName, @Email, @Birthday, @Position, @HighestLevel)";
        
        var rowsAffected = await ExecuteSqlAsync(sql, form);
        return rowsAffected == 0 ? "Something went wrong" : string.Empty;
    }
    
    public async Task<string> RemoveAdmittedUserAsync(BCAccount account)
    {
        string sql = @"UPDATE account
                        SET isactive = FALSE
                            WHERE email = @Email";
        
        var rowsAffected = await ExecuteSqlAsync(sql, account);
        return rowsAffected == 0 ? "Something went wrong" : string.Empty;
    }

    public async Task<IEnumerable<BCAccount>> GetAccountsAsync()
    {
        string sql = @"SELECT
                          *
                        FROM
                          account
                        WHERE isactive = TRUE";
        return await QueryDbAsync<BCAccount>(sql);
    }

    public async Task<BCAccount?> GetAccountByEmailAsync(string email)
    {
        string sql = @"SELECT
                          *
                        FROM
                          account
                        WHERE
                          email = @Email";
        return await QueryDbSingleAsync<BCAccount>(sql, new { Email = email });
    }

    public async Task<BCAccount?> GetAccountByIdAsync(int accountId)
    {
        string sql = @"SELECT
                          *
                        FROM
                          account
                        WHERE
                          id = @AccountId";
        return await QueryDbSingleAsync<BCAccount>(sql, new { AccountId = accountId });
    }

    public async Task UpdateAccountProfilePictureAsync(string email, string imagePath)
    {
        string sql = @"UPDATE account
                        SET
                          profilepicture = @ImagePath
                        WHERE
                          email = @Email";
        await ExecuteSqlAsync(sql, new { Email = email, ImagePath = imagePath });
    }

    public async Task<IEnumerable<BCTournament>> GetTournamentsAsync()
    {
        string sql = @"SELECT *
                        FROM tournament";
        return await QueryDbAsync<BCTournament>(sql);
    }    
    
    public async Task<BCTournament?> GetTournamentByYearAsync(string year)
    {
        string sql = @"SELECT *
                        FROM tournament
                        WHERE EXTRACT(YEAR FROM start_date) = @Year";
        return await QueryDbSingleAsync<BCTournament>(sql, new { Year = year });
    }

    public async Task<BCTournament?> GetCurrentTournamentAsync()
    {
        string sql = @"SELECT *
                        FROM tournament
                        WHERE current = TRUE";
        return await QueryDbSingleAsync<BCTournament>(sql);
    }
    
    public async Task SetUserAsPayedAsync(string email)
    {
        string sql = @"UPDATE account
                        SET payed = true
                        WHERE email = @Email";
        
        await ExecuteSqlAsync(sql, new { Email = email });
    }

    public async Task ConfigPlayerProfileAsync(RegisterFormModel form, int tournamentId)
    {
        
        // first check to see if a user in this tournament exists - only proceed if there is none
        string checkSql = @"SELECT p.*
                            FROM players p
                                     INNER JOIN account a ON p.account_id = a.id AND a.email = @Email
                            WHERE tournament_id = @TournamentId";
        var result = await QueryDbSingleAsync<PlayerProfile>(checkSql, new { Email = form.Email, TournamentId = tournamentId });
        if (result is not null) return;

        // get account
        var account = await GetAccountByEmailAsync(form.Email);
        if (account is null) return;
        
        string sql = @"INSERT INTO players(name, dob, preferred_beer, account_id, team_id, jersey_number, position, champion, tournament_id)
                        VALUES (@Name, @Dob, NULL, @AccountId, NULL, NULL, @Position, FALSE, @TournamentId)";
        
        await ExecuteSqlAsync(sql, new
        {
            Name = $"{form.FirstName} {form.LastName}",
            Dob = form.Birthday!.Value.ToString("yyyy-MM-dd"),
            AccountId = account.id,
            Position = form.Position,
            TournamentId = tournamentId,
        });
    }

    public async Task<BCDraftPick?> GetMostRecentDraftPickAsync(int draftId)
    {
        string sql = @"WITH max_round AS (SELECT MAX(round)
                       FROM draftpick
                       WHERE draft_id = @DraftId),
                             max_round_picks AS (SELECT * FROM draftpick WHERE round IN (SELECT * FROM max_round) AND draft_id = @DraftId)
                        SELECT *
                            FROM draftpick d
                                     INNER JOIN max_round_picks m ON d.id = m.id AND d.pick = (SELECT MAX(pick) FROM max_round_picks)";
        
        return await QueryDbSingleAsync<BCDraftPick>(sql, new { DraftId = draftId });
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
        
        return await QueryDbSingleAsync<BCTeam>(sql, new { DraftId = draftId, Order = order });
    }

    public async Task<IEnumerable<BCTeam>> GetTeamsInTournamentAsync(int tournamentId)
    {
        string sql = @"SELECT *
                        FROM team
                        WHERE tournament_id = @TournamentId";
        
        return await QueryDbAsync<BCTeam>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<BCDraftOrder>> GetDraftOrderAsync(int draftId)
    {
        string sql = @"SELECT *
                        FROM draftorder
                        WHERE draft_id = @DraftId";
        
        return await QueryDbAsync<BCDraftOrder>(sql, new { DraftId = draftId });
    }

    public async Task DraftPlayerAsync(PlayerProfile player, BCTeam team, BCDraftPick draftPick)
    {
        
        string playerSql = @"UPDATE players
                        SET team_id = @TeamId
                            WHERE id = @PlayerId
                            AND tournament_id = @TournamentId";
        await ExecuteSqlAsync(playerSql, new { TeamId = team.id, PlayerId = player.id, TournamentId = player.tournament_id });
        
        draftPick.player_id = player.id;
        string draftSql = @"INSERT INTO draftpick (draft_id, round, pick, player_id)
                                VALUES (@draft_id, @round, @pick, @player_id)";
        await ExecuteSqlAsync(draftSql, draftPick);
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
        
        return await QueryDbAsync<BCDraftPickDetail>(sql, new { DraftId = draftId });
    }

    public async Task ResetDraftAsync(int draftId)
    {
        
        // delete all draft picks in draft
        string draftSql = @"DELETE
                                FROM draftpick
                                WHERE draft_id = @DraftId
                                RETURNING player_id";
        var deletedPicks = await QueryDbAsync<int>(draftSql, new { DraftId = draftId });
        
        // set all player teams to null
        {
            string sql = @"UPDATE players
                                    SET team_id = NULL
                                        WHERE tournament_id = (SELECT tournament_id FROM draft WHERE id = @DraftId)
                                        AND id = ANY(@PlayerIds)";
            await ExecuteSqlAsync(sql, new
            {
                DraftId = draftId,
                PlayerIds = deletedPicks.ToList(),
            });
        }
    }


    public async Task<IEnumerable<BCSponsor>> GetTournamentSponsorsAsync(int tournamentId)
    {
        string sql = @"SELECT *
                        FROM sponsor
                        WHERE tournament_id = @TournamentId";
        return await QueryDbAsync<BCSponsor>(sql, new { TournamentId = tournamentId });
    }

}


