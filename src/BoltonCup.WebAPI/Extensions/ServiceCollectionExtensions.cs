using System.Threading.RateLimiting;
using BoltonCup.Core;
using BoltonCup.WebAPI.Authentication;
using BoltonCup.WebAPI.Filters;
using BoltonCup.WebAPI.Mapping.Auth;
using BoltonCup.WebAPI.Mapping.Core;
using BoltonCup.WebAPI.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.WebAPI;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        return services
            .AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblyContaining<EntityBase>();
    }

    private static IServiceCollection AddAuthServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyConstants.Scheme, null);
        
        return builder.Services
            .ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".BoltonCup.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                if (builder.Environment.IsProduction())
                {
                    options.Cookie.Domain = ".boltoncup.ca";
                }

                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            })
            .AddAuthorization(options => 
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, ApiKeyConstants.Scheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
    }
    
    private static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
        services.AddCors(options => 
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(
                        "http://localhost:5239",
                        "https://localhost:7244",
                        "https://localhost:7266",
                        "https://localhost:7269",
                        "https://boltoncup.ca",
                        "https://www.boltoncup.ca",
                        "https://auth.boltoncup.ca"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        return services;
    }

    private static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
    {
        return services
            .Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                    Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
            })
            .AddRateLimiter(options => 
            {
                options.RejectionStatusCode = 429;
                options.GlobalLimiter = GlobalRateLimiter.Create();
                options.AddPolicy<string, StrictEmailCheckPolicy>(nameof(StrictEmailCheckPolicy));
            });
    }

    private static IServiceCollection AddMappers(this IServiceCollection services)
    {
        return services
            .AddTransient<IBriefMapper, BriefMapper>()
            .AddTransient<IGameMapper, GameMapper>()
            .AddTransient<IGoalieGameLogMapper, GoalieGameLogMapper>()
            .AddTransient<IGoalieStatMapper, GoalieStatMapper>()
            .AddTransient<IInfoGuideMapper, InfoGuideMapper>()
            .AddTransient<IPlayerMapper, PlayerMapper>()
            .AddTransient<ISkaterGameLogMapper, SkaterGameLogMapper>()
            .AddTransient<ISkaterStatMapper, SkaterStatMapper>()
            .AddTransient<ITeamMapper, TeamMapper>()
            .AddTransient<ITournamentMapper, TournamentMapper>()
            .AddTransient<IUserMapper, UserMapper>();
    }
    
    public static IServiceCollection AddBoltonCupWebAPIServices(this WebApplicationBuilder builder)
    {
        builder
            .AddAuthServices()
            .AddFluentValidationServices()
            .AddCorsServices()
            .AddRateLimitingServices()
            .AddRouting(options => options.LowercaseUrls = true)
            .AddMappers()
            .AddControllers(options => options.Filters.Add<ApiExceptionFilterAttribute>());
        return builder.Services;
    }
}