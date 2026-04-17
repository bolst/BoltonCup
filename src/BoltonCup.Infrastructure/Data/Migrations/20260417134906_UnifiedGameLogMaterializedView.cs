using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedGameLogMaterializedView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS core.mv_skater_game_stats;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS core.mv_goalie_game_stats;");
            
            migrationBuilder.DropTable(
                name: "goalie_game_logs",
                schema: "core");

            migrationBuilder.DropTable(
                name: "skater_game_logs",
                schema: "core");

            migrationBuilder.Sql(@"CREATE MATERIALIZED VIEW core.mv_skater_game_logs
                            AS
                             WITH roster AS (
                                     SELECT g_1.id AS game_id,
                                        p_1.id AS player_id,
                                        p_1.team_id,
                                            CASE
                                                WHEN g_1.home_team_id = p_1.team_id THEN g_1.away_team_id
                                                ELSE g_1.home_team_id
                                            END AS opponent_team_id
                                       FROM core.games g_1
                                         JOIN core.players p_1 ON p_1.team_id = g_1.home_team_id OR p_1.team_id = g_1.away_team_id
                                      WHERE p_1.""position"" <> 'goalie'::text
                                    ), p_goals AS (
                                     SELECT goals.game_id,
                                        goals.goal_player_id AS player_id,
                                        count(goals.id) AS goals
                                       FROM core.goals
                                      WHERE goals.goal_player_id IS NOT NULL
                                      GROUP BY goals.game_id, goals.goal_player_id
                                    ), p_assists AS (
                                     SELECT a_1.game_id,
                                        a_1.player_id,
                                        count(a_1.id) AS assists
                                       FROM ( SELECT goals.id,
                                                goals.game_id,
                                                goals.assist1_player_id AS player_id
                                               FROM core.goals
                                              WHERE goals.assist1_player_id IS NOT NULL
                                            UNION ALL
                                             SELECT goals.id,
                                                goals.game_id,
                                                goals.assist2_player_id AS player_id
                                               FROM core.goals
                                              WHERE goals.assist2_player_id IS NOT NULL) a_1
                                      GROUP BY a_1.game_id, a_1.player_id
                                    ), p_penalties AS (
                                     SELECT penalties.game_id,
                                        penalties.player_id,
                                        COALESCE(sum(penalties.duration_mins), 0::bigint) AS penalty_minutes
                                       FROM core.penalties
                                      GROUP BY penalties.game_id, penalties.player_id
                                    )
                             SELECT r.player_id,
                                1 AS games_played,
                                COALESCE(g_stats.goals, 0::bigint) AS goals,
                                COALESCE(a_stats.assists, 0::bigint) AS assists,
                                COALESCE(g_stats.goals, 0::bigint) + COALESCE(a_stats.assists, 0::bigint) AS points,
                                COALESCE(pen.penalty_minutes, 0::bigint) AS penalty_minutes,
                                a.id AS account_id,
                                a.first_name,
                                a.last_name,
                                p.""position"",
                                p.jersey_number,
                                a.birthday,
                                a.avatar_key AS profile_picture,
                                r.team_id,
                                team.name AS team_name,
                                team.name_short AS team_name_short,
                                team.abbreviation AS team_abbreviation,
                                team.logo_key AS team_logo_url,
                                r.opponent_team_id AS opponent_id,
                                opp.name AS opponent_name,
                                opp.name_short AS opponent_name_short,
                                opp.abbreviation AS opponent_abbreviation,
                                opp.logo_key AS opponent_logo_url,
                                r.game_id,
                                g.game_time,
                                g.game_type,
                                g.venue AS game_venue,
                                g.rink AS game_rink,
                                g.tournament_id,
                                tr.name AS tournament_name,
                                tr.is_active AS tournament_active
                               FROM roster r
                                 LEFT JOIN p_goals g_stats ON r.game_id = g_stats.game_id AND r.player_id = g_stats.player_id
                                 LEFT JOIN p_assists a_stats ON r.game_id = a_stats.game_id AND r.player_id = a_stats.player_id
                                 LEFT JOIN p_penalties pen ON r.game_id = pen.game_id AND r.player_id = pen.player_id
                                 JOIN core.players p ON r.player_id = p.id
                                 JOIN core.accounts a ON p.account_id = a.id
                                 JOIN core.games g ON r.game_id = g.id
                                 JOIN core.teams team ON r.team_id = team.id
                                 JOIN core.teams opp ON r.opponent_team_id = opp.id
                                 JOIN core.tournaments tr ON g.tournament_id = tr.id;");
            
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX ix_mv_skater_game_stats_game_player ON core.mv_skater_game_logs (game_id, player_id);");
            
            migrationBuilder.Sql(@"CREATE MATERIALIZED VIEW core.mv_goalie_game_logs
                                    AS
                                     WITH roster AS (
                                             SELECT g_1.id AS game_id,
                                                p_1.id AS player_id,
                                                p_1.team_id,
                                                    CASE
                                                        WHEN g_1.home_team_id = p_1.team_id THEN g_1.away_team_id
                                                        ELSE g_1.home_team_id
                                                    END AS opponent_team_id
                                               FROM core.games g_1
                                                 JOIN core.players p_1 ON p_1.team_id = g_1.home_team_id OR p_1.team_id = g_1.away_team_id
                                              WHERE p_1.""position"" = 'goalie'::text
                                            ), team_goals AS (
                                             SELECT goals.game_id,
                                                goals.team_id,
                                                count(goals.id) AS goals
                                               FROM core.goals
                                              GROUP BY goals.game_id, goals.team_id
                                            ), p_goals AS (
                                             SELECT goals.game_id,
                                                goals.goal_player_id AS player_id,
                                                count(goals.id) AS goals
                                               FROM core.goals
                                              WHERE goals.goal_player_id IS NOT NULL
                                              GROUP BY goals.game_id, goals.goal_player_id
                                            ), p_assists AS (
                                             SELECT a_1.game_id,
                                                a_1.player_id,
                                                count(a_1.id) AS assists
                                               FROM ( SELECT goals.id,
                                                        goals.game_id,
                                                        goals.assist1_player_id AS player_id
                                                       FROM core.goals
                                                      WHERE goals.assist1_player_id IS NOT NULL
                                                    UNION ALL
                                                     SELECT goals.id,
                                                        goals.game_id,
                                                        goals.assist2_player_id AS player_id
                                                       FROM core.goals
                                                      WHERE goals.assist2_player_id IS NOT NULL) a_1
                                              GROUP BY a_1.game_id, a_1.player_id
                                            ), p_penalties AS (
                                             SELECT penalties.game_id,
                                                penalties.player_id,
                                                COALESCE(sum(penalties.duration_mins), 0::bigint) AS penalty_minutes
                                               FROM core.penalties
                                              GROUP BY penalties.game_id, penalties.player_id
                                            )
                                     SELECT r.player_id,
                                        COALESCE(opp_goals.goals, 0::bigint) AS goals_against,
                                        0 AS shots_against,
                                        0 AS saves,
                                            CASE
                                                WHEN COALESCE(opp_goals.goals, 0::bigint) = 0 THEN 1
                                                ELSE 0
                                            END AS shutouts,
                                            CASE
                                                WHEN COALESCE(my_goals.goals, 0::bigint) > COALESCE(opp_goals.goals, 0::bigint) THEN 1
                                                ELSE 0
                                            END AS wins,
                                        0.0 AS save_percentage,
                                        COALESCE(opp_goals.goals, 0::bigint)::numeric AS goals_against_average,
                                        1 AS games_played,
                                        COALESCE(g_stats.goals, 0::bigint) AS goals,
                                        COALESCE(a_stats.assists, 0::bigint) AS assists,
                                        COALESCE(g_stats.goals, 0::bigint) + COALESCE(a_stats.assists, 0::bigint) AS points,
                                        COALESCE(pen.penalty_minutes, 0::bigint) AS penalty_minutes,
                                        a.id AS account_id,
                                        a.first_name,
                                        a.last_name,
                                        p.""position"",
                                        p.jersey_number,
                                        a.birthday,
                                        a.avatar_key AS profile_picture,
                                        r.team_id,
                                        team.name AS team_name,
                                        team.name_short AS team_name_short,
                                        team.abbreviation AS team_abbreviation,
                                        team.logo_key AS team_logo_url,
                                        r.opponent_team_id AS opponent_id,
                                        opp.name AS opponent_name,
                                        opp.name_short AS opponent_name_short,
                                        opp.abbreviation AS opponent_abbreviation,
                                        opp.logo_key AS opponent_logo_url,
                                        r.game_id,
                                        g.game_time,
                                        g.game_type,
                                        g.venue AS game_venue,
                                        g.rink AS game_rink,
                                        g.tournament_id,
                                        tr.name AS tournament_name,
                                        tr.is_active AS tournament_active
                                       FROM roster r
                                         LEFT JOIN team_goals my_goals ON r.game_id = my_goals.game_id AND r.team_id = my_goals.team_id
                                         LEFT JOIN team_goals opp_goals ON r.game_id = opp_goals.game_id AND r.opponent_team_id = opp_goals.team_id
                                         LEFT JOIN p_goals g_stats ON r.game_id = g_stats.game_id AND r.player_id = g_stats.player_id
                                         LEFT JOIN p_assists a_stats ON r.game_id = a_stats.game_id AND r.player_id = a_stats.player_id
                                         LEFT JOIN p_penalties pen ON r.game_id = pen.game_id AND r.player_id = pen.player_id
                                         JOIN core.players p ON r.player_id = p.id
                                         JOIN core.accounts a ON p.account_id = a.id
                                         JOIN core.games g ON r.game_id = g.id
                                         JOIN core.teams team ON r.team_id = team.id
                                         JOIN core.teams opp ON r.opponent_team_id = opp.id
                                         JOIN core.tournaments tr ON g.tournament_id = tr.id;");
            
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX ix_mv_goalie_game_stats_game_player ON core.mv_goalie_game_logs (game_id, player_id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "goalie_game_logs",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    opponent_team_id = table.Column<int>(type: "integer", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    assists = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    goals = table.Column<int>(type: "integer", nullable: false),
                    goals_against = table.Column<int>(type: "integer", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true),
                    penalty_minutes = table.Column<double>(type: "double precision", nullable: false),
                    saves = table.Column<int>(type: "integer", nullable: false),
                    shots_against = table.Column<int>(type: "integer", nullable: false),
                    shutout = table.Column<bool>(type: "boolean", nullable: false),
                    win = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goalie_game_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_goalie_game_logs_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_goalie_game_logs_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_goalie_game_logs_teams_opponent_team_id",
                        column: x => x.opponent_team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_goalie_game_logs_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skater_game_logs",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    opponent_team_id = table.Column<int>(type: "integer", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    assists = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    goals = table.Column<int>(type: "integer", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true),
                    penalty_minutes = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skater_game_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_skater_game_logs_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_skater_game_logs_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_skater_game_logs_teams_opponent_team_id",
                        column: x => x.opponent_team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_skater_game_logs_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_goalie_game_logs_game_id",
                schema: "core",
                table: "goalie_game_logs",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_goalie_game_logs_opponent_team_id",
                schema: "core",
                table: "goalie_game_logs",
                column: "opponent_team_id");

            migrationBuilder.CreateIndex(
                name: "IX_goalie_game_logs_player_id",
                schema: "core",
                table: "goalie_game_logs",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_goalie_game_logs_team_id",
                schema: "core",
                table: "goalie_game_logs",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_skater_game_logs_game_id",
                schema: "core",
                table: "skater_game_logs",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_skater_game_logs_opponent_team_id",
                schema: "core",
                table: "skater_game_logs",
                column: "opponent_team_id");

            migrationBuilder.CreateIndex(
                name: "IX_skater_game_logs_player_id",
                schema: "core",
                table: "skater_game_logs",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_skater_game_logs_team_id",
                schema: "core",
                table: "skater_game_logs",
                column: "team_id");
            
            
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS core.mv_skater_game_logs;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS core.mv_goalie_game_logs;");

            migrationBuilder.Sql(@"CREATE MATERIALIZED VIEW core.mv_skater_game_stats
                            AS
                              SELECT gl.player_id,
                            1 AS games_played,
                            gl.goals,
                            gl.assists,
                            gl.goals + gl.assists AS points,
                            gl.penalty_minutes,
                            a.id AS account_id,
                            a.first_name,
                            a.last_name,
                            p.""position"",
                            p.jersey_number,
                            a.birthday,
                            a.avatar_key AS profile_picture,
                            gl.team_id,
                            team.name AS team_name,
                            team.name_short AS team_name_short,
                            team.abbreviation AS team_abbreviation,
                            team.logo_key AS team_logo_url,
                            gl.opponent_team_id AS opponent_id,
                            opp.name AS opponent_name,
                            opp.name_short AS opponent_name_short,
                            opp.abbreviation AS opponent_abbreviation,
                            opp.logo_key AS opponent_logo_url,
                            gl.game_id,
                            g.game_time,
                            g.game_type,
                            g.venue AS game_venue,
                            g.rink AS game_rink,
                            g.tournament_id,
                            tr.name AS tournament_name,
                            tr.is_active AS tournament_active
                           FROM core.skater_game_logs gl
                             JOIN core.players p ON gl.player_id = p.id
                             JOIN core.accounts a ON p.account_id = a.id
                             JOIN core.games g ON gl.game_id = g.id
                             JOIN core.teams team ON gl.team_id = team.id
                             JOIN core.teams opp ON gl.opponent_team_id = opp.id
                             JOIN core.tournaments tr ON g.tournament_id = tr.id;");
            
            migrationBuilder.Sql(@"CREATE MATERIALIZED VIEW core.mv_goalie_game_stats
                                    AS
                                      SELECT gl.player_id,
                                    gl.goals_against,
                                    gl.shots_against,
                                    gl.saves,
                                        CASE
                                            WHEN gl.shutout THEN 1
                                            ELSE 0
                                        END AS shutouts,
                                        CASE
                                            WHEN gl.win THEN 1
                                            ELSE 0
                                        END AS wins,
                                        CASE
                                            WHEN gl.shots_against = 0 THEN 0::double precision
                                            ELSE gl.saves::double precision / gl.shots_against::double precision
                                        END AS save_percentage,
                                    gl.goals_against::double precision AS goals_against_average,
                                    1 AS games_played,
                                    gl.goals,
                                    gl.assists,
                                    gl.goals + gl.assists AS points,
                                    gl.penalty_minutes,
                                    a.id AS account_id,
                                    a.first_name,
                                    a.last_name,
                                    p.""position"",
                                    p.jersey_number,
                                    a.birthday,
                                    a.avatar_key AS profile_picture,
                                    gl.team_id,
                                    team.name AS team_name,
                                    team.name_short AS team_name_short,
                                    team.abbreviation AS team_abbreviation,
                                    team.logo_key AS team_logo_url,
                                    gl.opponent_team_id AS opponent_id,
                                    opp.name AS opponent_name,
                                    opp.name_short AS opponent_name_short,
                                    opp.abbreviation AS opponent_abbreviation,
                                    opp.logo_key AS opponent_logo_url,
                                    gl.game_id,
                                    g.game_time,
                                    g.game_type,
                                    g.venue AS game_venue,
                                    g.rink AS game_rink,
                                    g.tournament_id,
                                    tr.name AS tournament_name,
                                    tr.is_active AS tournament_active
                                   FROM core.goalie_game_logs gl
                                     JOIN core.players p ON gl.player_id = p.id
                                     JOIN core.accounts a ON p.account_id = a.id
                                     JOIN core.games g ON gl.game_id = g.id
                                     JOIN core.teams team ON gl.team_id = team.id
                                     JOIN core.teams opp ON gl.opponent_team_id = opp.id
                                     JOIN core.tournaments tr ON g.tournament_id = tr.id;");
        }
    }
}
