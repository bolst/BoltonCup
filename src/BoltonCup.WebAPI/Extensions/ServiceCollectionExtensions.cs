using System.Threading.RateLimiting;
using BoltonCup.Core.Queries.Base;
using BoltonCup.WebAPI.Authentication;
using BoltonCup.WebAPI.Filters;
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
            .AddValidatorsFromAssemblyContaining<DefaultPaginationQuery>();
    }

    private static IServiceCollection AddAuthServices(this IServiceCollection services, IHostEnvironment environment)
    {
        services
            .AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyConstants.Scheme, null);
        
        return services
            .ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".BoltonCup.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                if (environment.IsProduction())
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
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    string partitionKey = context.User.Identity?.Name ??
                                          context.Connection.RemoteIpAddress?.ToString() ?? 
                                          "anon";
                    return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100, 
                        SegmentsPerWindow = 4,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2,
                    });
                });
            });
    }
    
    public static IServiceCollection AddBoltonCupWebAPIServices(this IServiceCollection services, IHostEnvironment environment)
    {
        services
            .AddFluentValidationServices()
            .AddAuthServices(environment)
            .AddCorsServices()
            .AddRateLimitingServices()
            .AddRouting(options => options.LowercaseUrls = true)
            .AddControllers(options => options.Filters.Add<ApiExceptionFilterAttribute>());
        return services;
    }
}