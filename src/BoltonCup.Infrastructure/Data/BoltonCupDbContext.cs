using System.Reflection;
using System.Runtime.Serialization;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoltonCup.Infrastructure.Data;

public class BoltonCupDbContext(DbContextOptions<BoltonCupDbContext> options)
    : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Core.BracketChallenge.Event> BracketChallenges { get; set; }
    public DbSet<Core.BracketChallenge.Registration> BracketChallengeRegistrations { get; set; }
    public DbSet<Draft> Drafts { get; set; }
    public DbSet<DraftOrder> DraftOrders { get; set; }
    public DbSet<DraftPick> DraftPicks { get; set; }
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameHighlight> GameHighlights { get; set; }
    public DbSet<GameStar> GameStars { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<GoalieStat> GoalieStats { get; set; }
    public DbSet<InfoGuide> InfoGuides { get; set; }
    public DbSet<Penalty> Penalties { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<SkaterStat> SkaterStats { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<TournamentBudgetItem> TournamentBudgetItems { get; set; }
    public DbSet<TournamentRegistration> TournamentRegistrations { get; set; }
    public DbSet<TournamentPlayerInfo> TournamentPlayerInfos { get; set; }
    public DbSet<TournamentPlayerGameAvailability> TournamentPlayerGameAvailabilities { get; set; }
    public DbSet<TournamentMusicTrack> TournamentMusicTracks { get; set; }
    public DbSet<TournamentSponsor> TournamentSponsors { get; set; }

    public DbSet<PlayerDraftRanking> PlayerDraftRankings { get; set; }
    public DbSet<PlayerFavourite> PlayerFavourites { get; set; }
    public DbSet<CustomRanking> CustomRankings { get; set; }
    public DbSet<CustomRankingPlayer> CustomRankingPlayers { get; set; }
    public DbSet<CustomRankingShare> CustomRankingShares { get; set; }
    public DbSet<SentEmail> SentEmails { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }
    public DbSet<EmailDraft> EmailDrafts { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<TradePlayer> TradePlayers { get; set; }
    public DbSet<TeamGeneralManager> TeamGeneralManagers { get; set; }
    public DbSet<Referee> Referees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");

        modelBuilder.Entity<Account>(entity =>
        {
            entity
                .ToTable("accounts")
                .HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.HighestLevel).HasColumnName("highest_level");
            entity.Property(e => e.Avatar).HasColumnName("avatar_key").HasDefaultValue(AssetUrlResolver.StaticKeys.PlayerAvatar);
            entity.Property(e => e.Banner).HasColumnName("banner_key").HasDefaultValue(AssetUrlResolver.StaticKeys.PlayerBanner);
            entity.Property(e => e.PreferredBeer).HasColumnName("preferred_beer");
            entity.Property(e => e.HeightFeet).HasColumnName("height_feet");
            entity.Property(e => e.HeightInches).HasColumnName("height_inches");
            entity.Property(e => e.Weight).HasColumnName("weight");
        });

        modelBuilder.Entity<Core.BracketChallenge.Event>(entity =>
        {
            entity
                .ToTable("bracket_challenge_events")
                .HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Link).HasColumnName("link");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Fee).HasColumnName("fee");
            entity.Property(e => e.IsOpen).HasColumnName("is_open");
            entity.Property(e => e.Logo).HasColumnName("logo");
            entity.Property(e => e.RegistrationCloseDate).HasColumnName("registration_close_date");
            entity.Property(e => e.TermsOfServiceMarkdownContent).HasColumnName("terms_of_service_markdown_content");
        });

        modelBuilder.Entity<Core.BracketChallenge.Registration>(entity =>
        {
            entity
                .ToTable("bracket_challenge_registrations")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.BracketChallenge)
                .WithMany(e => e.Registrations)
                .HasForeignKey(e => e.EventId);
            entity
                .HasIndex(e => new
                {
                    e.EventId, e.Email
                })
                .IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.AgreedToTermsOfService).HasColumnName("agreed_terms_of_service");
        });

        modelBuilder.Entity<Draft>(entity =>
        {
            entity
                .ToTable("drafts")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Tournament)
                .WithMany()
                .HasForeignKey(e => e.TournamentId);
            entity
                .HasOne(e => e.DraftOwner)
                .WithMany()
                .HasForeignKey(e => e.DraftOwnerAccountId)
                .IsRequired(false);
            entity
                .HasOne(e => e.DefaultCustomRanking)
                .WithMany()
                .HasForeignKey(e => e.DefaultCustomRankingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.Title).HasColumnName("draft_title");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Type).HasColumnName("draft_type").HasConversion(
                e => e.ToString()!.ToLower(),
                s => Enum.Parse<DraftType>(s, true)
            );
            entity.Property(e => e.Status).HasColumnName("draft_status").HasConversion(
                e => e.ToString()!.ToLower(),
                s => Enum.Parse<DraftStatus>(s, true)
            );
            entity.Property(e => e.IsVisible).HasColumnName("is_visible").HasDefaultValue(false);
            entity.Property(e => e.DraftOwnerAccountId).HasColumnName("draft_owner_account_id");
            entity.Property(e => e.Rounds).HasColumnName("rounds").HasDefaultValue(0);
            entity.Property(e => e.Teams).HasColumnName("teams").HasDefaultValue(0);
            entity.Property(e => e.SecondsPerPick).HasColumnName("seconds_per_pick").HasDefaultValue(120);
            entity.Property(e => e.DefaultCustomRankingId).HasColumnName("default_custom_ranking_id");
        });

        modelBuilder.Entity<DraftOrder>(entity =>
        {
            entity
                .ToTable("draft_orders")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Draft)
                .WithMany(e => e.DraftOrders)
                .HasForeignKey(e => e.DraftId);
            entity
                .HasOne(e => e.Team)
                .WithMany()
                .HasForeignKey(e => e.TeamId);
            entity
                .HasIndex(e => new
                {
                    e.DraftId, e.TeamId
                })
                .IsUnique();
            entity
                .HasIndex(e => new
                {
                    e.DraftId, e.Pick
                })
                .IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.DraftId).HasColumnName("draft_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Pick).HasColumnName("pick_number");
            entity.Property(e => e.AutoPick).HasColumnName("auto_pick");
        });

        modelBuilder.Entity<DraftPick>(entity =>
        {
            entity
                .ToTable("draft_picks")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Draft)
                .WithMany(e => e.DraftPicks)
                .HasForeignKey(e => e.DraftId);
            entity
                .HasOne(e => e.Team)
                .WithMany()
                .HasForeignKey(e => e.TeamId);
            entity
                .HasOne(e => e.Player)
                .WithMany()
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasIndex(e => new
                {
                    e.DraftId, e.OverallPick
                })
                .IsUnique();
            entity
                .HasIndex(e => new
                {
                    e.DraftId, e.PlayerId
                })
                .IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.DraftId).HasColumnName("draft_id");
            entity.Property(e => e.OverallPick).HasColumnName("overall_pick_number");
            entity.Property(e => e.Round).HasColumnName("round_number");
            entity.Property(e => e.RoundPick).HasColumnName("round_pick_number");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.ClockStartedAt).HasColumnName("clock_started_at");
            entity.Property(e => e.IsAutoPick).HasColumnName("is_auto_pick").HasDefaultValue(false);
            entity.Property(e => e.Version).HasColumnName("row_version").IsRowVersion();
        });

        modelBuilder.Entity<Gallery>(entity =>
        {
            entity
                .ToTable("galleries")
                .HasKey(e => e.Id);
            entity
                .HasIndex(e => e.Title)
                .IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(80);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Source).HasColumnName("source");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity
                .ToTable("games")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Tournament)
                .WithMany(e => e.Games)
                .HasForeignKey(e => e.TournamentId);
            entity
                .HasOne(e => e.HomeTeam)
                .WithMany(e => e.HomeGames)
                .HasForeignKey(e => e.HomeTeamId);
            entity
                .HasOne(e => e.AwayTeam)
                .WithMany(e => e.AwayGames)
                .HasForeignKey(e => e.AwayTeamId);
            entity
                .HasMany(g => g.Referees)
                .WithMany(r => r.Games)
                .UsingEntity(
                    "game_referees",
                    l => l.HasOne(typeof(Referee)).WithMany().HasForeignKey("referee_id"),
                    r => r.HasOne(typeof(Game)).WithMany().HasForeignKey("game_id"),
                    j => j.ToTable("game_referees"));
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.GameTime).HasColumnName("game_time");
            entity.Property(e => e.HomeTeamId).HasColumnName("home_team_id");
            entity.Property(e => e.AwayTeamId).HasColumnName("away_team_id");
            entity.Property(e => e.HomeTeamPlaceholder).HasColumnName("home_team_placeholder");
            entity.Property(e => e.AwayTeamPlaceholder).HasColumnName("away_team_placeholder");
            entity.Property(e => e.GameType).HasColumnName("game_type")
                .HasConversion(new EnumMemberConverter<GameType>())
                .HasDefaultValue(GameType.RoundRobin);
            entity.Property(e => e.GameState).HasColumnName("game_state")
                .HasConversion(new EnumMemberConverter<GameState>())
                .HasDefaultValue(GameState.Pending);
            entity.Property(e => e.Venue).HasColumnName("venue");
            entity.Property(e => e.Rink).HasColumnName("rink");
        });

        modelBuilder.Entity<GameHighlight>(entity =>
        {
            entity
                .ToTable("game_highlights")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Game)
                .WithMany(g => g.Highlights)
                .HasForeignKey(e => e.GameId);
            entity
                .HasOne(e => e.Player)
                .WithMany(p => p.GameHighlights)
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasIndex(e => e.VideoId)
                .IsUnique();
            entity
                .HasIndex(e => e.GameId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<GameStar>(entity =>
        {
            entity
                .ToTable("game_stars")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Game)
                .WithMany(g => g.Stars)
                .HasForeignKey(e => e.GameId);
            entity
                .HasOne(e => e.Player)
                .WithMany(p => p.Stars)
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasIndex(e => new
                {
                    e.PlayerId, e.GameId
                })
                .IsUnique();
            entity
                .HasIndex(e => new
                {
                    e.StarRank, e.GameId
                })
                .IsUnique();
            entity.HasIndex(e => e.GameId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
        });

        modelBuilder.Entity<PlayerFavourite>(entity =>
        {
            entity
                .ToTable("player_favourites")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId);
            entity
                .HasOne(e => e.Draft)
                .WithMany()
                .HasForeignKey(e => e.DraftId);
            entity
                .HasOne(e => e.Player)
                .WithMany()
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasIndex(e => new
                {
                    e.AccountId, e.DraftId, e.PlayerId
                })
                .IsUnique();
            entity.HasIndex(e => new
            {
                e.AccountId, e.DraftId
            });
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.DraftId).HasColumnName("draft_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
        });

        modelBuilder.Entity<CustomRanking>(entity =>
        {
            entity
                .ToTable("custom_rankings")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId);
            entity
                .HasOne(e => e.Tournament)
                .WithMany()
                .HasForeignKey(e => e.TournamentId);
            entity
                .HasMany(e => e.Players)
                .WithOne(e => e.CustomRanking)
                .HasForeignKey(e => e.CustomRankingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new
            {
                e.AccountId, e.TournamentId
            });
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.Title).HasColumnName("title");
        });

        modelBuilder.Entity<CustomRankingPlayer>(entity =>
        {
            entity
                .ToTable("custom_ranking_players")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Player)
                .WithMany()
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasIndex(e => new
                {
                    e.CustomRankingId, e.PlayerId
                })
                .IsUnique();
            entity.HasIndex(e => e.CustomRankingId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CustomRankingId).HasColumnName("custom_ranking_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.GamesPlayed).HasColumnName("games_played");
            entity.Property(e => e.TotalPoints).HasColumnName("total_points");
        });

        modelBuilder.Entity<CustomRankingShare>(entity =>
        {
            entity
                .ToTable("custom_ranking_shares")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.CustomRanking)
                .WithMany(e => e.SharedWith)
                .HasForeignKey(e => e.CustomRankingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(e => e.SharedWithAccount)
                .WithMany()
                .HasForeignKey(e => e.SharedWithAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasIndex(e => new
                {
                    e.CustomRankingId, e.SharedWithAccountId
                })
                .IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CustomRankingId).HasColumnName("custom_ranking_id");
            entity.Property(e => e.SharedWithAccountId).HasColumnName("shared_with_account_id");
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity
                .ToTable("goals")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Team)
                .WithMany(e => e.Goals)
                .HasForeignKey(e => e.TeamId);
            entity
                .HasOne(e => e.Game)
                .WithMany(e => e.Goals)
                .HasForeignKey(e => e.GameId);
            entity
                .HasOne(e => e.Scorer)
                .WithMany(e => e.Goals)
                .HasForeignKey(e => e.GoalPlayerId);
            entity
                .HasOne(e => e.Assist1Player)
                .WithMany(e => e.PrimaryAssists)
                .HasForeignKey(e => e.Assist1PlayerId);
            entity
                .HasOne(e => e.Assist2Player)
                .WithMany(e => e.SecondaryAssists)
                .HasForeignKey(e => e.Assist2PlayerId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Period).HasColumnName("period_number");
            entity.Property(e => e.PeriodLabel).HasColumnName("period_label");
            entity.Property(e => e.PeriodTimeRemaining).HasColumnName("period_time_remaining");
            entity.Property(e => e.GoalPlayerId).HasColumnName("goal_player_id");
            entity.Property(e => e.Assist1PlayerId).HasColumnName("assist1_player_id");
            entity.Property(e => e.Assist2PlayerId).HasColumnName("assist2_player_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
        });

        modelBuilder.Entity<GoalieStat>(entity =>
        {
            entity
                .ToView("mv_goalie_game_logs")
                .HasKey(e => new
                {
                    e.GameId, e.PlayerId
                });
            entity
                .HasOne(e => e.Player)
                .WithMany(e => e.GoalieGameLogs)
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasOne(e => e.Team)
                .WithMany(e => e.GoalieGameLogs)
                .HasForeignKey(e => e.TeamId);
            entity
                .HasOne(e => e.Opponent)
                .WithMany()
                .HasForeignKey(e => e.OpponentId);
            entity
                .HasOne(e => e.Game)
                .WithMany(e => e.GoalieGameLogs)
                .HasForeignKey(e => e.GameId);
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.GoalsAgainst).HasColumnName("goals_against");
            entity.Property(e => e.ShotsAgainst).HasColumnName("shots_against");
            entity.Property(e => e.Saves).HasColumnName("saves");
            entity.Property(e => e.Shutouts).HasColumnName("shutouts");
            entity.Property(e => e.Wins).HasColumnName("wins");
            entity.Property(e => e.SavePercentage).HasColumnName("save_percentage");
            entity.Property(e => e.GoalsAgainstAverage).HasColumnName("goals_against_average");
            entity.Property(e => e.GamesPlayed).HasColumnName("games_played");
            entity.Property(e => e.Goals).HasColumnName("goals");
            entity.Property(e => e.Assists).HasColumnName("assists");
            entity.Property(e => e.Points).HasColumnName("points");
            entity.Property(e => e.PenaltyMinutes).HasColumnName("penalty_minutes");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.JerseyNumber).HasColumnName("jersey_number");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.ProfilePicture).HasColumnName("profile_picture");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.TeamName).HasColumnName("team_name");
            entity.Property(e => e.TeamNameShort).HasColumnName("team_name_short");
            entity.Property(e => e.TeamAbbreviation).HasColumnName("team_abbreviation");
            entity.Property(e => e.TeamLogoUrl).HasColumnName("team_logo_url");
            entity.Property(e => e.OpponentId).HasColumnName("opponent_id");
            entity.Property(e => e.OpponentName).HasColumnName("opponent_name");
            entity.Property(e => e.OpponentNameShort).HasColumnName("opponent_name_short");
            entity.Property(e => e.OpponentAbbreviation).HasColumnName("opponent_abbreviation");
            entity.Property(e => e.OpponentLogoUrl).HasColumnName("opponent_logo_url");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.GameTime).HasColumnName("game_time");
            entity.Property(e => e.GameType).HasColumnName("game_type");
            entity.Property(e => e.GameVenue).HasColumnName("game_venue");
            entity.Property(e => e.GameRink).HasColumnName("game_rink");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.TournamentName).HasColumnName("tournament_name");
            entity.Property(e => e.TournamentActive).HasColumnName("tournament_active");
        });

        modelBuilder.Entity<InfoGuide>(entity =>
        {
            entity
                .ToTable("info_guides")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Tournament)
                .WithOne(e => e.InfoGuide)
                .HasForeignKey<InfoGuide>(e => e.TournamentId);
            entity
                .HasIndex(e => e.TournamentId)
                .HasFilter("tournament_id IS NOT NULL")
                .IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.MarkdownContent).HasColumnName("markdown_content");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
        });

        modelBuilder.Entity<Penalty>(entity =>
        {
            entity
                .ToTable("penalties")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Team)
                .WithMany(e => e.Penalties)
                .HasForeignKey(e => e.TeamId);
            entity
                .HasOne(e => e.Game)
                .WithMany(e => e.Penalties)
                .HasForeignKey(e => e.GameId);
            entity
                .HasOne(e => e.Player)
                .WithMany(e => e.Penalties)
                .HasForeignKey(e => e.PlayerId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Period).HasColumnName("period_number");
            entity.Property(e => e.PeriodLabel).HasColumnName("period_label");
            entity.Property(e => e.PeriodTimeRemaining).HasColumnName("period_time_remaining");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.InfractionName).HasColumnName("infraction_name");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_mins");
            entity.Property(e => e.Notes).HasColumnName("notes");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("players")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Account)
                .WithMany(e => e.Players)
                .HasForeignKey(e => e.AccountId);
            entity
                .HasOne(e => e.Tournament)
                .WithMany(e => e.Players)
                .HasForeignKey(e => e.TournamentId);
            entity
                .HasOne(e => e.Team)
                .WithMany(e => e.Players)
                .HasForeignKey(e => e.TeamId);
            entity
                .HasIndex(e => new
                {
                    e.AccountId, e.TournamentId
                })
                .IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.JerseyNumber).HasColumnName("jersey_number");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.JerseySize).HasColumnName("jersey_size");
            entity.Property(e => e.CanPlayEitherPosition).HasColumnName("can_play_either_position");
            entity.Property(e => e.Friends).HasColumnName("friends");
            entity.Property(e => e.AgreedToCodeOfConduct).HasColumnName("agreed_code_of_conduct");
            entity.Property(e => e.AgreedToConcussionWaiver).HasColumnName("agreed_concussion_waiver");
            entity.Property(e => e.AgreedToCommunicationConsent).HasColumnName("agreed_communication_consent");
            entity.Property(e => e.Captaincy).HasColumnName("captaincy")
                .HasConversion(new EnumMemberConverter<Captaincy>())
                .HasDefaultValue(Captaincy.None);
        });

        modelBuilder.Entity<Trade>(entity =>
        {
            entity
                .ToTable("trades")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Tournament)
                .WithMany()
                .HasForeignKey(e => e.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity
                .HasOne(e => e.ProposingTeam)
                .WithMany()
                .HasForeignKey(e => e.ProposingTeamId)
                .OnDelete(DeleteBehavior.Restrict);
            entity
                .HasOne(e => e.ReceivingTeam)
                .WithMany()
                .HasForeignKey(e => e.ReceivingTeamId)
                .OnDelete(DeleteBehavior.Restrict);
            entity
                .HasIndex(e => new
                {
                    e.TournamentId, e.Status
                });
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.ProposingTeamId).HasColumnName("proposing_team_id");
            entity.Property(e => e.ReceivingTeamId).HasColumnName("receiving_team_id");
            entity.Property(e => e.Status).HasColumnName("status")
                .HasConversion(new EnumMemberConverter<TradeStatus>())
                .HasDefaultValue(TradeStatus.Pending);
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.CreatedByAccountId).HasColumnName("created_by_account_id");
            entity.Property(e => e.RespondedByAccountId).HasColumnName("responded_by_account_id");
            entity.Property(e => e.RespondedAt).HasColumnName("responded_at");
            entity.Property(e => e.ResolvedByAccountId).HasColumnName("resolved_by_account_id");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
        });

        modelBuilder.Entity<TradePlayer>(entity =>
        {
            entity
                .ToTable("trade_players")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Trade)
                .WithMany(t => t.Players)
                .HasForeignKey(e => e.TradeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(e => e.Player)
                .WithMany()
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            // A player may be in at most one open (Pending/Accepted) trade at a time.
            entity
                .HasIndex(e => e.PlayerId)
                .IsUnique()
                .HasFilter("is_locked");
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TradeId).HasColumnName("trade_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.FromTeamId).HasColumnName("from_team_id");
            entity.Property(e => e.ToTeamId).HasColumnName("to_team_id");
            entity.Property(e => e.IsLocked).HasColumnName("is_locked").HasDefaultValue(true);
        });

        modelBuilder.Entity<PlayerDraftRanking>(entity =>
        {
            entity
                .ToTable("draft_rankings")
                .HasKey(e => e.Id);
            entity
                .HasIndex(e => new
                {
                    e.PlayerId, e.DraftId
                })
                .IsUnique();
            entity
                .HasIndex(e => e.DraftId);
            entity
                .HasOne(e => e.Player)
                .WithMany()
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasOne(e => e.Draft)
                .WithMany()
                .HasForeignKey(e => e.DraftId);
            entity
                .HasOne(e => e.Tournament)
                .WithMany()
                .HasForeignKey(e => e.TournamentId);
            entity
                .HasOne(e => e.DraftPick)
                .WithMany()
                .HasForeignKey(e => e.DraftPickId);
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.DraftId).HasColumnName("draft_id");
            entity.Property(e => e.DraftPickId).HasColumnName("draft_pick_id");
            entity.Property(e => e.GamesPlayed).HasColumnName("games_played");
            entity.Property(e => e.TotalPoints).HasColumnName("total_points");
            entity.Property(e => e.IsChampion).HasColumnName("is_champion");
            entity.Property(e => e.DraftRanking).HasColumnName("draft_ranking");
            entity.Property(e => e.OverrideRanking).HasColumnName("override_ranking");
            entity.Property(e => e.IsExcluded).HasColumnName("is_excluded").HasDefaultValue(false);
        });

        modelBuilder.Entity<SkaterStat>(entity =>
        {
            entity
                .ToView("mv_skater_game_logs")
                .HasKey(e => new
                {
                    e.GameId, e.PlayerId
                });
            entity
                .HasOne(e => e.Player)
                .WithMany(e => e.SkaterGameLogs)
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasOne(e => e.Team)
                .WithMany(e => e.SkaterGameLogs)
                .HasForeignKey(e => e.TeamId);
            entity
                .HasOne(e => e.Opponent)
                .WithMany()
                .HasForeignKey(e => e.OpponentId);
            entity
                .HasOne(e => e.Game)
                .WithMany(e => e.SkaterGameLogs)
                .HasForeignKey(e => e.GameId);
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.GamesPlayed).HasColumnName("games_played");
            entity.Property(e => e.Goals).HasColumnName("goals");
            entity.Property(e => e.Assists).HasColumnName("assists");
            entity.Property(e => e.Points).HasColumnName("points");
            entity.Property(e => e.PenaltyMinutes).HasColumnName("penalty_minutes");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.JerseyNumber).HasColumnName("jersey_number");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.ProfilePicture).HasColumnName("profile_picture");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.TeamName).HasColumnName("team_name");
            entity.Property(e => e.TeamNameShort).HasColumnName("team_name_short");
            entity.Property(e => e.TeamAbbreviation).HasColumnName("team_abbreviation");
            entity.Property(e => e.TeamLogoUrl).HasColumnName("team_logo_url");
            entity.Property(e => e.OpponentId).HasColumnName("opponent_id");
            entity.Property(e => e.OpponentName).HasColumnName("opponent_name");
            entity.Property(e => e.OpponentNameShort).HasColumnName("opponent_name_short");
            entity.Property(e => e.OpponentAbbreviation).HasColumnName("opponent_abbreviation");
            entity.Property(e => e.OpponentLogoUrl).HasColumnName("opponent_logo_url");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.GameTime).HasColumnName("game_time");
            entity.Property(e => e.GameType).HasColumnName("game_type");
            entity.Property(e => e.GameVenue).HasColumnName("game_venue");
            entity.Property(e => e.GameRink).HasColumnName("game_rink");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.TournamentName).HasColumnName("tournament_name");
            entity.Property(e => e.TournamentActive).HasColumnName("tournament_active");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity
                .ToTable("teams")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Tournament)
                .WithMany(e => e.Teams)
                .HasForeignKey(e => e.TournamentId);
            entity
                .HasMany(e => e.GeneralManagers)
                .WithMany(a => a.ManagedTeams)
                .UsingEntity<TeamGeneralManager>(
                    j => j.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId),
                    j => j.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId),
                    j =>
                    {
                        j.ToTable("team_general_managers");
                        j.HasKey(x => new { x.TeamId, x.AccountId });
                        j.Property(x => x.TeamId).HasColumnName("team_id");
                        j.Property(x => x.AccountId).HasColumnName("account_id");
                        j.Property(x => x.TournamentId).HasColumnName("tournament_id");
                        j.HasOne<Tournament>().WithMany().HasForeignKey(x => x.TournamentId);
                        // Enforce that an account can be GM of at most one team per tournament.
                        j.HasIndex(x => new { x.TournamentId, x.AccountId }).IsUnique();
                    });
            // Team songs reference library tracks; deleting a track just clears the reference.
            entity
                .HasOne(e => e.GoalSongTrack)
                .WithMany()
                .HasForeignKey(e => e.GoalSongTrackId)
                .OnDelete(DeleteBehavior.SetNull);
            entity
                .HasOne(e => e.WinSongTrack)
                .WithMany()
                .HasForeignKey(e => e.WinSongTrackId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.NameShort).HasColumnName("name_short");
            entity.Property(e => e.Abbreviation).HasColumnName("abbreviation");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.Logo).HasColumnName("logo_key");
            entity.Property(e => e.Banner).HasColumnName("banner_key");
            entity.Property(e => e.PrimaryColorHex).HasColumnName("primary_hex");
            entity.Property(e => e.SecondaryColorHex).HasColumnName("secondary_hex");
            entity.Property(e => e.TertiaryColorHex).HasColumnName("tertiary_hex");
            entity.Property(e => e.GoalSongTrackId).HasColumnName("goal_song_track_id");
            entity.Property(e => e.WinSongTrackId).HasColumnName("win_song_track_id");
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity
                .ToTable("tournaments")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.WinningTeam)
                .WithMany()
                .HasForeignKey(e => e.WinningTeamId);
            entity
                .HasOne(e => e.Gallery)
                .WithMany(t => t.Tournaments)
                .HasForeignKey(e => e.GalleryId)
                .OnDelete(DeleteBehavior.SetNull);
            entity
                .HasIndex(e => e.IsActive)
                .HasFilter("is_active = TRUE")
                .IsUnique();
            entity
                .HasIndex(e => e.IsStatsFeatured)
                .HasFilter("is_stats_featured = TRUE")
                .IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Logo).HasColumnName("logo_key");
            entity.Property(e => e.BackgroundImage).HasColumnName("background_image_key");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.WinningTeamId).HasColumnName("winning_team_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsRegistrationOpen).HasColumnName("is_registration_open");
            entity.Property(e => e.IsPaymentOpen).HasColumnName("is_payment_open");
            entity.Property(e => e.IsPlayerInfoOpen).HasColumnName("is_player_info_open");
            entity.Property(e => e.IsTradingOpen).HasColumnName("is_trading_open");
            entity.Property(e => e.SkaterPaymentLink).HasColumnName("skater_payment_link");
            entity.Property(e => e.GoaliePaymentLink).HasColumnName("goalie_payment_link");
            entity.Property(e => e.SkaterLimit).HasColumnName("skater_limit");
            entity.Property(e => e.GoalieLimit).HasColumnName("goalie_limit");
            entity.Property(e => e.SkaterRegistrationFee).HasColumnName("skater_registration_fee");
            entity.Property(e => e.GoalieRegistrationFee).HasColumnName("goalie_registration_fee");
            entity.Property(e => e.GalleryId).HasColumnName("gallery_id");
            entity.Property(e => e.IsStatsFeatured).HasColumnName("is_stats_featured");
            entity.Property(e => e.FeaturedStatsLabel).HasColumnName("featured_stats_label");
        });

        modelBuilder.Entity<TournamentBudgetItem>(entity =>
        {
            entity
                .ToTable("tournament_budget_items")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Tournament)
                .WithMany(e => e.Expenses)
                .HasForeignKey(e => e.TournamentId);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.BudgetItemType).HasColumnName("budget_item_type").HasConversion(new EnumMemberConverter<BudgetItemType>());
        });

        modelBuilder.Entity<TournamentRegistration>(entity =>
        {
            entity
                .ToTable("tournament_registrations")
                .HasKey(e => e.Id);
            entity
                .HasIndex(e => new
                {
                    e.AccountId, e.TournamentId
                })
                .IsUnique();
            entity
                .HasOne(e => e.Tournament)
                .WithMany(a => a.Registrations)
                .HasForeignKey(e => e.TournamentId);
            entity
                .HasOne(e => e.Account)
                .WithMany(a => a.TournamentRegistrations)
                .HasForeignKey(e => e.AccountId);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CurrentStep).HasColumnName("current_step");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.IsComplete).HasColumnName("is_complete");
        });

        modelBuilder.Entity<TournamentPlayerInfo>(entity =>
        {
            entity
                .ToTable("tournament_player_info")
                .HasKey(e => e.Id);
            entity
                .HasIndex(e => new
                {
                    e.AccountId, e.TournamentId
                })
                .IsUnique();
            entity
                .HasOne(e => e.Tournament)
                .WithMany(a => a.PlayerInfos)
                .HasForeignKey(e => e.TournamentId);
            entity
                .HasOne(e => e.Account)
                .WithMany(a => a.TournamentPlayerInfos)
                .HasForeignKey(e => e.AccountId);
            entity
                .HasMany(e => e.GameAvailabilities)
                .WithOne(e => e.TournamentPlayerInfo)
                .HasForeignKey(e => e.TournamentPlayerInfoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.SongTrackId).HasColumnName("song_track_id");
            entity.Property(e => e.SongName).HasColumnName("song_name");
            entity.Property(e => e.SongArtist).HasColumnName("song_artist");
            entity.Property(e => e.SongAlbumArtUrl).HasColumnName("song_album_art_url");
        });

        modelBuilder.Entity<TournamentMusicTrack>(entity =>
        {
            entity
                .ToTable("tournament_music_tracks")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Tournament)
                .WithMany(t => t.MusicTracks)
                .HasForeignKey(e => e.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
            // Unique on the match key; Postgres treats NULL track ids as distinct, so untagged uploads coexist.
            entity.HasIndex(e => new
            {
                e.TournamentId, e.ProviderType, e.TrackId
            }).IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.AudioFileKey).HasColumnName("audio_file_key");
            entity.Property(e => e.TrackId).HasColumnName("track_id");
            entity.Property(e => e.ProviderType).HasColumnName("provider_type")
                .HasConversion(new EnumMemberConverter<MusicProviderType>())
                .HasDefaultValue(MusicProviderType.Spotify);
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Artist).HasColumnName("artist");
            entity.Property(e => e.AlbumArtUrl).HasColumnName("album_art_url");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            // No HasDefaultValue on these value-type props: a DB-generated default would be applied
            // whenever the CLR value equals its default (Pending / false), silently overriding inserts.
            entity.Property(e => e.IsInBasePool).HasColumnName("is_in_base_pool");
            entity.Property(e => e.Status).HasColumnName("status")
                .HasConversion(new EnumMemberConverter<MusicTrackStatus>());
            entity.Property(e => e.Source).HasColumnName("source")
                .HasConversion(new EnumMemberConverter<MusicTrackSource>());
            entity.Property(e => e.RequestedByName).HasColumnName("requested_by_name");
        });

        modelBuilder.Entity<TournamentPlayerGameAvailability>(entity =>
        {
            entity
                .ToTable("tournament_player_game_availability")
                .HasKey(e => e.Id);
            entity
                .HasIndex(e => new
                {
                    e.TournamentPlayerInfoId, e.GameId
                })
                .IsUnique();
            entity.HasIndex(e => e.GameId);
            entity
                .HasOne(e => e.Game)
                .WithMany()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TournamentPlayerInfoId).HasColumnName("tournament_player_info_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Availability).HasColumnName("availability")
                .HasConversion(new EnumMemberConverter<GameAvailability>());
        });

        modelBuilder.Entity<TournamentSponsor>(entity =>
        {
            entity
                .ToTable("tournament_sponsors")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Tournament)
                .WithMany(e => e.Sponsors)
                .HasForeignKey(e => e.TournamentId);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.Name).HasColumnName("sponsor_name");
            entity.Property(e => e.Logo).HasColumnName("logo_key");
            entity.Property(e => e.WebsiteUrl).HasColumnName("website_url");
            entity.Property(e => e.SortKey).HasColumnName("sort_key");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<SentEmail>(entity =>
        {
            entity
                .ToTable("sent_emails")
                .HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.Audience).HasColumnName("audience");
            entity.Property(e => e.RecipientCount).HasColumnName("recipient_count");
            entity.Property(e => e.UseLayout).HasColumnName("use_layout");
            entity.Property(e => e.BroadcastId).HasColumnName("broadcast_id");
        });

        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity
                .ToTable("email_logs")
                .HasKey(e => e.Id);
            entity.HasIndex(e => e.BroadcastId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Recipient).HasColumnName("recipient");
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.TemplateName).HasColumnName("template_name");
            entity.Property(e => e.Succeeded).HasColumnName("succeeded");
            entity.Property(e => e.Error).HasColumnName("error");
            entity.Property(e => e.BroadcastId).HasColumnName("broadcast_id");
        });

        modelBuilder.Entity<EmailDraft>(entity =>
        {
            entity
                .ToTable("email_drafts")
                .HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.UseLayout).HasColumnName("use_layout");
            entity.Property(e => e.AudienceKind).HasColumnName("audience_kind");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
        });

        modelBuilder.Entity<Referee>(entity =>
        {
            entity
                .ToTable("referees")
                .HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Ignore(e => e.FullName);
        });

        // entities deriving from EntityBase should have created_at = now() by default
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsSubclassOf(typeof(EntityBase)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(EntityBase.CreatedAt))
                    .HasDefaultValueSql("now() AT TIME ZONE 'UTC'");
            }
        }
    }


    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<DateTimeWithKindConverter>();

        configurationBuilder
            .Properties<DateTime?>()
            .HaveConversion<NullableDateTimeWithKindConverter>();
    }
}
public class DateTimeWithKindConverter() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
);
public class NullableDateTimeWithKindConverter() : ValueConverter<DateTime?, DateTime?>(
    v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
);
public class EnumMemberConverter<TEnum>()
    : ValueConverter<TEnum, string>(
        s => GetEnumMemberValue(s),
        s => GetEnumFromValue(s)
    ) where TEnum : Enum
{
    public static string GetEnumMemberValue(TEnum value)
    {
        return typeof(TEnum)
                   .GetField(value.ToString())?
                   .GetCustomAttribute<EnumMemberAttribute>()?.Value
               ?? value.ToString();
    }

    public static TEnum GetEnumFromValue(string value)
    {
        foreach (var field in typeof(TEnum).GetFields())
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
            if (attribute?.Value == value || field.Name == value)
            {
                return (TEnum?)field.GetValue(null)
                       ?? throw new ArgumentException($"Unknown value: {value}");
            }
        }

        throw new ArgumentException($"Unknown value: {value}");
    }
}