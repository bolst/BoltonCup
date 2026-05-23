using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
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
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<BoltonCupUser>>();

            await RoleSeeder.SeedAdminUserAsync(roleManager, userManager, configuration);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

internal static class RoleSeeder
{
    internal static async Task SeedAdminUserAsync(
        RoleManager<IdentityRole> roleManager, 
        UserManager<BoltonCupUser> userManager, 
        IConfiguration configuration)
    {
        foreach (var roleName in BoltonCupRole.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var adminEmail = configuration["BoltonCup:AdminEmail"]; 
        if (string.IsNullOrEmpty(adminEmail)) 
            return; 

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}