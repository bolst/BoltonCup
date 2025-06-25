
namespace BoltonCup.Shared.Data;
using Npgsql;

public partial class BCData : DapperBase, IBCData
{
    private readonly ICacheService cacheService;
    private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);


    public BCData(string _connectionString, ICacheService _cacheService) : base(_connectionString)
    {
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
                        WHERE tournament_id = @TournamentId
                        ORDER BY birthday, p.position";
        
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
        string sql = @"SELECT *
                        FROM playergbg
                        WHERE account_id = @AccountId
                            {0}";

        sql = string.Format(sql, tournamentId is not null ? "and tournament_id = @TournamentId" : string.Empty);

        return await QueryDbAsync<PlayerGameSummary>(sql, new { AccountId = accountId, TournamentId = tournamentId });
    }

    public async Task<IEnumerable<GoalieGameSummary>> GetGoalieGameByGame(int accountId, int? tournamentId = null)
    {
        string sql = @"SELECT *
                        FROM goaliegbg
                        WHERE account_id = @AccountId
                            {0}";
        
        sql = string.Format(sql, tournamentId is not null ? "and tournament_id = @TournamentId" : string.Empty);

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

    public async Task<IEnumerable<PlayerStatLine>> GetPlayerStats(int tournamentId, int? teamId = null)
    {
        string sql = @"SELECT * from playerstats
                            WHERE tournament_id = @TournamentId
                                {0}";

        sql = string.Format(sql, teamId.HasValue ? "AND t.id = @TeamId" : string.Empty);
        
        return await QueryDbAsync<PlayerStatLine>(sql, new { TournamentId = tournamentId, TeamId = teamId });
    }
    public async Task<IEnumerable<GoalieStatLine>> GetGoalieStats(int tournamentId, int? teamId = null)
    {
        string sql = @"SELECT * from goaliestats
                            WHERE tournament_id = @TournamentId
                                {0}";

        sql = string.Format(sql, teamId.HasValue ? "AND t.id = @TeamId" : string.Empty);
        
        return await QueryDbAsync<GoalieStatLine>(sql, new { TournamentId = tournamentId, TeamId = teamId });
    }

    public async Task<IEnumerable<PlayerProfilePicture>> GetPlayerProfilePictures()
    {
        string sql = @"SELECT
                        player_id AS PlayerId,
                        uri AS Source
                        FROM profile_pictures;";
        return await QueryDbAsync<PlayerProfilePicture>(sql);
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
                        WHERE isactive = TRUE
                        ORDER BY lastname";
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
        
        string sql = @"INSERT INTO players(name, dob, account_id, team_id, jersey_number, position, champion, tournament_id)
                        VALUES (@Name, @Dob, @AccountId, NULL, NULL, @Position, FALSE, @TournamentId)";
        
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
    
    public async Task<IEnumerable<PlayerProfile>> GetDraftAvailableTournamentPlayersAsync(int tournamentId)
    {
        string sql = @"SELECT *, p.id
                        FROM players p
                                 LEFT OUTER JOIN account a ON p.account_id = a.id AND a.isactive = TRUE
                        WHERE tournament_id = @TournamentId
                            AND p.team_id IS NULL
                        ORDER BY birthday, p.position";
        return await QueryDbAsync<PlayerProfile>(sql, new { TournamentId = tournamentId });
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


    public async Task<IEnumerable<BCSponsor>> GetActiveSponsorsAsync()
    {
        string sql = @"SELECT *
                        FROM sponsor
                        WHERE is_active = true
                        ORDER BY sort_key";
        return await QueryDbAsync<BCSponsor>(sql);
    }


    public async Task<IEnumerable<BCGame>> GetIncompleteGamesAsync()
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
                        WHERE g.state != @State
                        ORDER BY g.date ASC";
        return await QueryDbAsync<BCGame>(sql, new { State = GameState.Complete });
    }

    public async Task<BCAccount?> GetAccountByPCKeyAsync(Guid pckey)
    {
        string sql = @"SELECT *
                        FROM account
                        WHERE pckey = @PCKey";
        
        return await QueryDbSingleAsync<BCAccount>(sql, new { PCKey = pckey });
    }
    
    
    public async Task<PlayerProfile?> GetCurrentPlayerProfileByPCKeyAsync(Guid pckey)
    {
        string sql = @"SELECT p.*
                        FROM players p
                                 INNER JOIN account a ON a.id = p.account_id
                                 INNER JOIN tournament t ON t.tournament_id = p.tournament_id AND t.current = TRUE
                        WHERE a.pckey = @PCKey";
        
        return await QueryDbSingleAsync<PlayerProfile>(sql, new { PCKey = pckey });
    }


    public async Task UpdateConfigDataAsync(BCAccount account)
    {
        string sql = @"UPDATE account
                        SET preferred_number1 = @preferred_number1,
                            preferred_number2 = @preferred_number2,
                            preferred_number3 = @preferred_number3,
                            preferred_beer = @preferred_beer,
                            songrequest = @songrequest,
                            songrequestid = @songrequestid
                        WHERE id = @id";

        await ExecuteSqlAsync(sql, account);
    }


    public async Task UpdatePlayerAvailabilityAsync(IEnumerable<BCAvailability> availabilities)
    {
        string sql = @"UPDATE availability
                        SET availability = @Availability
                            WHERE id = @Id";

        await ExecuteSqlAsync(sql, availabilities);
    }


    public async Task<IEnumerable<BCAvailability>> GetPlayerAvailabilityAsync(int accountId, int tournamentId)
    {
        string sql = @"WITH set_games AS (SELECT y.id,
                                              y.account_id                    AS accountid,
                                              y.game_id                       AS gameid,
                                              y.availability                  AS availability,
                                              g.date                          AS gamedate,
                                              CASE
                                                  WHEN g.home_team_id = p.team_id THEN g.home_team_id
                                                  ELSE g.away_team_id END     AS teamid,
                                              CASE
                                                  WHEN g.home_team_id = p.team_id THEN g.away_team_id
                                                  ELSE g.home_team_id END     AS opponentid,
                                              CASE
                                                  WHEN g.home_team_id = p.team_id THEN home_team.name
                                                  ELSE away_team.name END     AS teamname,
                                              CASE
                                                  WHEN g.home_team_id = p.team_id THEN away_team.name
                                                  ELSE home_team.name END     AS opponentname,
                                              CASE
                                                  WHEN g.home_team_id = p.team_id THEN home_team.logo_url
                                                  ELSE away_team.logo_url END AS teamlogo,
                                              CASE
                                                  WHEN g.home_team_id = p.team_id THEN away_team.logo_url
                                                  ELSE home_team.logo_url END AS opponentlogo,
                                           g.type as gametype
                                           FROM availability y
                                                    INNER JOIN account a ON y.account_id = a.id
                                                    INNER JOIN game g ON y.game_id = g.id AND g.tournament_id = @TournamentId
                                                    INNER JOIN players p ON p.account_id = y.account_id AND p.tournament_id = @TournamentId
                                                    INNER JOIN team home_team ON g.home_team_id = home_team.id
                                                    INNER JOIN team away_team ON g.away_team_id = away_team.id
                                           WHERE y.account_id = @AccountId),
                         tbd_games AS (SELECT
                                              y.id,
                                              y.account_id   AS accountid,
                                              y.game_id      AS gameid,
                                              y.availability AS availability,
                                              g.date         AS gamedate,
                                              0              AS teamid,
                                              0              AS opponentid,
                                              ''             AS teamname,
                                              ''             AS opponentname,
                                              ''             AS teamlogo,
                                              ''                opponentlogo,
                                              g.type as gametype
                                           FROM availability y
                                                    INNER JOIN account a ON y.account_id = a.id
                                                    INNER JOIN game g ON y.game_id = g.id AND g.tournament_id = @TournamentId AND
                                                                         g.home_team_id IS NULL AND g.away_team_id IS NULL
                                                    INNER JOIN
                                                players p
                                                ON p.account_id = y.account_id AND p.tournament_id = @TournamentId
                                           WHERE y.account_id = @AccountId)
                    SELECT *
                        FROM set_games
                    UNION
                    SELECT *
                        FROM tbd_games
                    ORDER BY gamedate";

        return await QueryDbAsync<BCAvailability>(sql, new { AccountId = accountId, TournamentId = tournamentId });
    }


    public async Task PopulatePlayerAvailabilitiesAsync(int accountId)
    {
        string sql = @"INSERT INTO availability(account_id, game_id, availability)
                        SELECT @AccountId,
                               g.id,
                               NULL
                            FROM game g
                                     INNER JOIN players p ON p.team_id IN (g.home_team_id, g.away_team_id) AND p.account_id = @AccountId
                                     INNER JOIN tournament t ON t.tournament_id = g.tournament_id AND t.current = TRUE
                        UNION
                        SELECT @AccountId,
                               g.id,
                               NULL
                            FROM game g
                                     INNER JOIN players p ON g.home_team_id IS NULL AND g.away_team_id IS NULL AND p.account_id = @AccountId
                                     INNER JOIN tournament t ON t.tournament_id = g.tournament_id AND t.current = TRUE
                        ON CONFLICT (account_id, game_id)
                            DO NOTHING";
                                
        await ExecuteSqlAsync(sql, new { AccountId = accountId });
    }


    public async Task<IEnumerable<BCGame>> GetActiveGamesAsync()
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
                        WHERE g.state = @State
                        ORDER BY g.date ASC";

        return await QueryDbAsync<BCGame>(sql, new { State = GameState.Active });
    }


    public async Task BeginRecordingGameAsync(int gameId)
    {
        string clearSql = $@"UPDATE game
                        SET state = '{GameState.PreGame}'
                            WHERE state = '{GameState.Active}'";

        await ExecuteSqlAsync(clearSql, new { GameId = gameId });
        
        string sql = $@"UPDATE game
                        SET state = '{GameState.Active}'
                            WHERE id = @GameId";

        await ExecuteSqlAsync(sql, new { GameId = gameId });
    }
    
    
    
    public async Task EndRecordingGameAsync(int gameId, bool complete = false)
    {
        string sql = @"UPDATE game
                        SET state = @State
                            WHERE id = @GameId";

        var state = complete ? GameState.Complete : GameState.PreGame;
        await ExecuteSqlAsync(sql, new { State = state, GameId = gameId });
    }
    
}


