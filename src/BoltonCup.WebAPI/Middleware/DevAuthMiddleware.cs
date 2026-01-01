using System.Security.Claims;

namespace BoltonCup.WebAPI.Middleware;

public class DevAuthMiddleware(RequestDelegate next)
{
    private const string DevUserId = "dev-user-id"; 
    private const string DevUserName = "Dev User";
    private const string DevUserEmail = "dev@localhost";

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Only run if no one is logged in yet
        if (context.User.Identity?.IsAuthenticated != true)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, DevUserId),
                new Claim(ClaimTypes.Name, DevUserName),
                new Claim(ClaimTypes.Email, DevUserEmail),
                // Optional: Add Roles if your app checks for them
                // new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "DevAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        // 2. Continue the pipeline
        await next(context);
    }
}