using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace BoltonCup.Infrastructure.Identity;

internal static class RoleSeeder
{
    internal static async Task SeedRolesAsync(
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
