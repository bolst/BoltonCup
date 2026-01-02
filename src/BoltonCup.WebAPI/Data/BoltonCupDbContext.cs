using BoltonCup.WebAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.WebAPI.Data;

public class BoltonCupDbContext(DbContextOptions<BoltonCupDbContext> options) 
    : DbContext(options)
{
    
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<Penalty> Penalties { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Tournament> Tournaments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("coretest");
        
        modelBuilder.Entity<Account>(entity =>
        {
            entity
                .ToTable("account")
                .HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.ProfilePicture).HasColumnName("profile_picture");
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
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.GameTime).HasColumnName("game_time");
            entity.Property(e => e.HomeTeamId).HasColumnName("home_team_id");
            entity.Property(e => e.AwayTeamId).HasColumnName("away_team_id");
            entity.Property(e => e.GameType).HasColumnName("game_type");
            entity.Property(e => e.Venue).HasColumnName("venue");
            entity.Property(e => e.Rink).HasColumnName("rink");
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity
                .ToTable("goals")
                .HasKey(e => e.Id);
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
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Period).HasColumnName("period_number");
            entity.Property(e => e.PeriodLabel).HasColumnName("period_label");
            entity.Property(e => e.PeriodTimeRemaining).HasColumnName("period_time_remaining");
            entity.Property(e => e.GoalPlayerId).HasColumnName("goal_player_id");
            entity.Property(e => e.Assist1PlayerId).HasColumnName("assist1_player_id");
            entity.Property(e => e.Assist2PlayerId).HasColumnName("assist2_player_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
        });

        modelBuilder.Entity<Penalty>(entity =>
        {
            entity
                .ToTable("penalties")
                .HasKey(e => e.Id);
            entity
                .HasOne(e => e.Game)
                .WithMany(e => e.Penalties)
                .HasForeignKey(e => e.GameId);
            entity
                .HasOne(e => e.Player)
                .WithMany(e => e.Penalties)
                .HasForeignKey(e => e.PlayerId);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
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
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.PreferredBeer).HasColumnName("preferred_beer");
            entity.Property(e => e.JerseyNumber).HasColumnName("jersey_number");
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
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.NameShort).HasColumnName("name_short");
            entity.Property(e => e.Abbreviation).HasColumnName("abbreviation");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.LogoUrl).HasColumnName("logo_url");
            entity.Property(e => e.BannerUrl).HasColumnName("banner_url");
            entity.Property(e => e.PrimaryColorHex).HasColumnName("primary_hex");
            entity.Property(e => e.SecondaryColorHex).HasColumnName("secondary_hex");
            entity.Property(e => e.TertiaryColorHex).HasColumnName("tertiary_hex");
            entity.Property(e => e.GoalSongUrl).HasColumnName("goal_song_url");
            entity.Property(e => e.PenaltySongUrl).HasColumnName("penalty_song_url");
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity
                .ToTable("tournaments")
                .HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
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
        });
    }
}
