using BoltonCup.Core;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.WebAPI.Authentication;
using BoltonCup.WebAPI.Controllers;
using BoltonCup.WebAPI.Filters;
using BoltonCup.WebAPI.Handlers;
using BoltonCup.WebAPI.Mapping;
using BoltonCup.WebAPI.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        return services
            .AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblyContaining<EntityBase>()
            .AddValidatorsFromAssemblyContaining<Controllers.BoltonCupControllerBase>();
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

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            })
            .AddAuthorization(options => 
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, ApiKeyConstants.Scheme)
                    .RequireAuthenticatedUser()
                    .Build();
                
                options.AddPolicy(BoltonCupPolicy.RequireCompletedAccount, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(BoltonCupClaimTypes.AccountId);
                });
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
            .AddTransient<IAccountMapper, AccountMapper>()
            .AddTransient<IGameMapper, GameMapper>()
            .AddTransient<IGoalieGameLogMapper, GoalieGameLogMapper>()
            .AddTransient<IGoalieStatMapper, GoalieStatMapper>()
            .AddTransient<IInfoGuideMapper, InfoGuideMapper>()
            .AddTransient<IPlayerMapper, PlayerMapper>()
            .AddTransient<ISkaterGameLogMapper, SkaterGameLogMapper>()
            .AddTransient<ISkaterStatMapper, SkaterStatMapper>()
            .AddTransient<ITeamMapper, TeamMapper>()
            .AddTransient<ITournamentMapper, TournamentMapper>()
            .AddTransient<ITournamentRegistrationMapper, TournamentRegistrationMapper>()
            .AddTransient<ITournamentPaymentMapper, TournamentPaymentMapper>()
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
            .AddProblemDetails()
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddControllers();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
                    );
                var response = new ApiErrorResponse
                {
                    Message = "One or more validation errors occurred",
                    Errors = errors
                };

                return new BadRequestObjectResult(response);
            };
        });
        
        return builder.Services;
    }
}