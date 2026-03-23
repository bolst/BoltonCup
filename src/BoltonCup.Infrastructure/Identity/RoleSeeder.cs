using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Infrastructure.Identity;

internal static class RoleSeeder
{
    internal static async Task SeedAdminUserAsync(
        RoleManager<IdentityRole> roleManager, 
        UserManager<BoltonCupUser> userManager, 
        IConfiguration configuration)
    {
        string[] roleNames = ["Admin", "User"];
        
        foreach (var roleName in roleNames)
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


public static class DatabaseSeederExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
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