using BoltonCup.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.WebAPI.Data;

public class BoltonCupDbContext : DbContext
{
    public BoltonCupDbContext(DbContextOptions<BoltonCupDbContext> options) : base(options)
    {
    }

    public DbSet<Team> Teams { get; set; }

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
    }
}
