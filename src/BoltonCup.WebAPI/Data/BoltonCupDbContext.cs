using BoltonCup.WebAPI.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.WebAPI.Data;

public class BoltonCupDbContext : DbContext
{
    public BoltonCupDbContext(DbContextOptions<BoltonCupDbContext> options) : base(options)
    {
    }

    public DbSet<Team> Teams { get; set; }
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<Player> Players { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams", "core");

            entity.HasKey(e => e.Id);

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
            entity.ToTable("tournaments", "core");

            entity.HasKey(e => e.Id);
            
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

        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("players", "core");

            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.PreferredBeer).HasColumnName("preferred_beer");
            entity.Property(e => e.JerseyNumber).HasColumnName("jersey_number");
        });
    }
}
