using BoltonCup.Core;
using BoltonCup.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoltonCup.Infrastructure.Data;


/*
 * dotnet ef migrations add [migration_name] --project ./BoltonCup.Infrastructure --startup-project ./BoltonCup.WebAPI -c BoltonCupDbContext
 * dotnet ef database update --project ./BoltonCup.Infrastructure --startup-project ./BoltonCup.WebAPI -c BoltonCupDbContext
 */

public class BoltonCupDbContext(DbContextOptions<BoltonCupDbContext> options) 
    : DbContext(options)
{
    
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Core.BracketChallenge.Event> BracketChallenges { get; set; }
    public DbSet<Core.BracketChallenge.Registration> BracketChallengeRegistrations { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameHighlight> GameHighlights { get; set; }
    public DbSet<GameStar> GameStars { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<GoalieGameLog> GoalieGameLogs { get; set; }
    public DbSet<GoalieStat> GoalieStats { get; set; }
    public DbSet<InfoGuide> InfoGuides { get; set; }
    public DbSet<Penalty> Penalties { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<SkaterGameLog> SkaterGameLogs { get; set; }
    public DbSet<SkaterStat> SkaterStats { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<TournamentRegistration> TournamentRegistrations { get; set; }
    
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
                .HasIndex(e => new { e.EventId, e.Email })
                .IsUnique();
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.AgreedToTermsOfService).HasColumnName("agreed_terms_of_service");
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
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.GameTime).HasColumnName("game_time");
            entity.Property(e => e.HomeTeamId).HasColumnName("home_team_id");
            entity.Property(e => e.AwayTeamId).HasColumnName("away_team_id");
            entity.Property(e => e.GameType).HasColumnName("game_type");
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
                .HasIndex(e => new { e.PlayerId, e.GameId })
                .IsUnique();
            entity
                .HasIndex(e => new { e.StarRank, e.GameId })
                .IsUnique();
            entity.HasIndex(e => e.GameId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
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
        
        modelBuilder.Entity<GoalieGameLog>(entity =>
        {
            entity
                .ToTable("goalie_game_logs")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Player)
                .WithMany(e => e.GoalieGameLogs)
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasOne(e => e.Team)
                .WithMany(e => e.GoalieGameLogs)
                .HasForeignKey(e => e.TeamId);
            entity
                .HasOne(e => e.OpponentTeam)
                .WithMany()
                .HasForeignKey(e => e.OpponentTeamId);
            entity
                .HasOne(e => e.Game)
                .WithMany(e => e.GoalieGameLogs)
                .HasForeignKey(e => e.GameId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.OpponentTeamId).HasColumnName("opponent_team_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Goals).HasColumnName("goals");
            entity.Property(e => e.Assists).HasColumnName("assists");
            entity.Property(e => e.PenaltyMinutes).HasColumnName("penalty_minutes");
            entity.Property(e => e.GoalsAgainst).HasColumnName("goals_against");
            entity.Property(e => e.ShotsAgainst).HasColumnName("shots_against");
            entity.Property(e => e.Saves).HasColumnName("saves");
            entity.Property(e => e.Shutout).HasColumnName("shutout");
            entity.Property(e => e.Win).HasColumnName("win");
        });
        
        modelBuilder.Entity<GoalieStat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("mv_goalie_game_stats");
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
                .HasIndex(e => new { e.AccountId, e.TournamentId })
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
        });

        modelBuilder.Entity<SkaterGameLog>(entity =>
        {
            entity
                .ToTable("skater_game_logs")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Player)
                .WithMany(e => e.SkaterGameLogs)
                .HasForeignKey(e => e.PlayerId);
            entity
                .HasOne(e => e.Team)
                .WithMany(e => e.SkaterGameLogs)
                .HasForeignKey(e => e.TeamId);
            entity
                .HasOne(e => e.OpponentTeam)
                .WithMany()
                .HasForeignKey(e => e.OpponentTeamId);
            entity
                .HasOne(e => e.Game)
                .WithMany(e => e.SkaterGameLogs)
                .HasForeignKey(e => e.GameId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.OpponentTeamId).HasColumnName("opponent_team_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Goals).HasColumnName("goals");
            entity.Property(e => e.Assists).HasColumnName("assists");
            entity.Property(e => e.PenaltyMinutes).HasColumnName("penalty_minutes");
        });

        modelBuilder.Entity<SkaterStat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("mv_skater_game_stats");
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
                .HasOne(e => e.GeneralManager)
                .WithMany(e => e.ManagedTeams)
                .HasForeignKey(e => e.GmAccountId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.NameShort).HasColumnName("name_short");
            entity.Property(e => e.Abbreviation).HasColumnName("abbreviation");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.GmAccountId).HasColumnName("gm_account_id");
            entity.Property(e => e.Logo).HasColumnName("logo_key");
            entity.Property(e => e.Banner).HasColumnName("banner_key");
            entity.Property(e => e.PrimaryColorHex).HasColumnName("primary_hex");
            entity.Property(e => e.SecondaryColorHex).HasColumnName("secondary_hex");
            entity.Property(e => e.TertiaryColorHex).HasColumnName("tertiary_hex");
            entity.Property(e => e.GoalSong).HasColumnName("goal_song_key");
            entity.Property(e => e.PenaltySong).HasColumnName("penalty_song_key");
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
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Logo).HasColumnName("logo_key");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.WinningTeamId).HasColumnName("winning_team_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsRegistrationOpen).HasColumnName("is_registration_open");
            entity.Property(e => e.IsPaymentOpen).HasColumnName("is_payment_open");
            entity.Property(e => e.SkaterPaymentLink).HasColumnName("skater_payment_link");
            entity.Property(e => e.GoaliePaymentLink).HasColumnName("goalie_payment_link");
            entity.Property(e => e.SkaterLimit).HasColumnName("skater_limit");
            entity.Property(e => e.GoalieLimit).HasColumnName("goalie_limit");
            entity.Property(e => e.SkaterRegistrationFee).HasColumnName("skater_registration_fee");
            entity.Property(e => e.GoalieRegistrationFee).HasColumnName("goalie_registration_fee");
        });

        modelBuilder.Entity<TournamentRegistration>(entity =>
        {
            entity
                .ToTable("tournament_registrations")
                .HasKey(e => e.Id) ;
            entity
                .HasIndex(e => new { e.AccountId, e.TournamentId })
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