using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeDbAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var coreContextFactory = services.GetRequiredService<IDbContextFactory<BoltonCupDbContext>>();
            var authContextFactory = services.GetRequiredService<IDbContextFactory<AuthDbContext>>();

            await using var coreContext = await coreContextFactory.CreateDbContextAsync();
            await using var authContext = await authContextFactory.CreateDbContextAsync();

            await coreContext.Database.MigrateAsync();
            await authContext.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<BoltonCupUser>>();

            await RoleSeeder.SeedRolesAsync(roleManager, userManager, configuration);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization skipped: {ex.Message}");
        }
    }
}
