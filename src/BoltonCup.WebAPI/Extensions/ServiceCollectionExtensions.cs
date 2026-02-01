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

    private static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyConstants.Scheme, null);
        return services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(IdentityConstants.BearerScheme, ApiKeyConstants.Scheme)
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
                        "https://boltoncup.ca"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        return services;
    }

    private static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
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
    
    public static IServiceCollection AddBoltonCupWebAPIServices(this IServiceCollection services)
    {
        services
            .AddFluentValidationServices()
            .AddAuthServices()
            .AddCorsServices()
            .AddRateLimitingServices()
            .AddRouting(options => options.LowercaseUrls = true)
            .AddControllers(options => options.Filters.Add<ApiExceptionFilterAttribute>());
        return services;
    }
}