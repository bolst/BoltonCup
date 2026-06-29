using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Data;

// This context is ONLY for Identity tables
public class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<BoltonCupUser, IdentityRole, string>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Put all auth tables in the "auth" schema
        builder.HasDefaultSchema("auth");
    }
}