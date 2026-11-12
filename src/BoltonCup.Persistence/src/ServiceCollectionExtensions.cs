using BoltonCup.Persistence.Data;
using BoltonCup.Persistence.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Persistence;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddBoltonCupDataProtection(this WebApplicationBuilder builder)
    {
        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<AuthDbContext>()
            .SetApplicationName("BoltonCup.SharedAuth");

        return builder;
    }

    public static WebApplicationBuilder AddBoltonCupPersistence(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddIdentityCore<BoltonCupUser>(ConfigureIdentity)
            .AddRoles<IdentityRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<BoltonCupClaimsPrincipalFactory>();

        var connectionString = builder.Configuration.GetValue<string>(ConfigurationPaths.ConnectionString);
        builder.Services
            .AddDbContextFactory<BoltonCupDbContext>(options => options.UseNpgsql(connectionString))
            .AddDbContextFactory<AuthDbContext>(options => options.UseNpgsql(connectionString));

        return builder;
    }

    internal static void ConfigureIdentity(IdentityOptions options)
    {
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        options.SignIn.RequireConfirmedAccount = true;
        // Wrong guesses are capped per caller in the API, so a stranger can't lock someone out.
        options.Lockout.AllowedForNewUsers = false;
    }
}

public static class ConfigurationPaths
{
    public const string ConnectionString = "BoltonCup:ConnectionString";
}