using System.Text.Json.Serialization;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Auth;
using BoltonCup.WebAPI.Errors;
using BoltonCup.WebAPI.Extensions;
using BoltonCup.WebAPI.Mapping;
using BoltonCup.WebAPI.RateLimiting;
using BoltonCup.WebAPI.Stripe;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI;

/// <summary>Extension methods for registering BoltonCup WebAPI services with the DI container.</summary>
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
            // Override the default scheme (set to Identity.Bearer by AddIdentityApiEndpoints) with a
            // forwarder: requests with the API key header use the API key scheme, everything else uses
            // the cookie scheme browsers/WASM authenticate with. It's one or the other per request.
            .AddAuthentication(options => options.DefaultScheme = ApiKeyConstants.ForwardingScheme)
            .AddPolicyScheme(ApiKeyConstants.ForwardingScheme, ApiKeyConstants.ForwardingScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                    context.Request.Headers.ContainsKey(ApiKeyConstants.Header)
                        ? ApiKeyConstants.Scheme
                        : IdentityConstants.ApplicationScheme;
            })
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
            .Configure<SecurityStampValidatorOptions>(options =>
            {
                // The security-stamp validator periodically rebuilds the principal from the user, which
                // would drop the masquerade marker claims. Carry them forward so a masquerade survives.
                options.OnRefreshingPrincipal = context =>
                {
                    if (context.NewPrincipal?.Identities.FirstOrDefault() is not { } identity)
                    {
                        return Task.CompletedTask;
                    }

                    foreach (var claimType in new[]
                             {
                                 BoltonCupClaimTypes.OriginalUserId, BoltonCupClaimTypes.OriginalUserName
                             })
                    {
                        if (context.CurrentPrincipal?.FindFirst(claimType) is { } claim && !identity.HasClaim(claim.Type, claim.Value))
                        {
                            identity.AddClaim(claim);
                        }
                    }

                    return Task.CompletedTask;
                };
            })
            .AddScoped<IAuthorizationHandler, DraftAccessHandler>()
            .AddScoped<IAuthorizationHandler, DraftManagerHandler>()
            .AddScoped<IAuthorizationHandler, TeamManagerHandler>()
            .AddScoped<IAuthorizationHandler, RankingManagerHandler>()
            .AddScoped<IAuthorizationHandler, RankingAccessHandler>()
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

                options.AddPolicy(BoltonCupPolicy.CanAccessDraft, policy =>
                    policy.Requirements.Add(new AccessDraftRequirement()));

                options.AddPolicy(BoltonCupPolicy.CanManageTeam, policy =>
                    policy.Requirements.Add(new ManageTeamRequirement()));

                options.AddPolicy(BoltonCupPolicy.CanManageDraft, policy =>
                    policy.Requirements.Add(new ManageDraftRequirement()));

                options.AddPolicy(BoltonCupPolicy.CanAccessRanking, policy =>
                    policy.Requirements.Add(new AccessRankingRequirement()));

                options.AddPolicy(BoltonCupPolicy.CanManageRanking, policy =>
                    policy.Requirements.Add(new ManageRankingRequirement()));
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
                        "http://localhost:5170",
                        "https://localhost:7244",
                        "https://localhost:7266",
                        "https://localhost:7269",
                        "https://localhost:7047",
                        "https://localhost:7055",
                        "https://boltoncup.ca",
                        "https://www.boltoncup.ca",
                        "https://auth.boltoncup.ca",
                        "https://draft.boltoncup.ca",
                        "https://timekeeper.boltoncup.ca"
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
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            })
            .AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = GlobalRateLimiter.Create();
                options.AddPolicy<string, StrictEmailCheckPolicy>(nameof(StrictEmailCheckPolicy));

                options.OnRejected = (context, cancellationToken) =>
                    RateLimitResponder.WriteResponseAsync(context, cancellationToken: cancellationToken);
            });
    }

    private static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddTransient<IMapper, Mapper>();
        return services;
    }

    private static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        return services
            .AddProblemDetails()
            .PostConfigure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problem = new BoltonCupValidationProblemDetails(context.ModelState.ToErrorDictionary())
                    {
                        Type = ErrorTypes.Validation, Title = "One or more validation errors occurred", Status = StatusCodes.Status400BadRequest, Instance = context.HttpContext.Request.Path,
                    };

                    return new BadRequestObjectResult(problem)
                    {
                        ContentTypes =
                        {
                            "application/problem+json"
                        }
                    };
                };
            })
            .AddExceptionHandler<BoltonCupExceptionHandler>()
            .AddExceptionHandler<UnhandledExceptionHandler>();
    }

    private static IServiceCollection AddSignalRServices(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }

    /// <summary>Registers all BoltonCup WebAPI services including auth, validation, CORS, rate limiting, and controllers.</summary>
    public static IServiceCollection AddBoltonCupWebAPIServices(this WebApplicationBuilder builder)
    {
        builder
            .AddAuthServices()
            .AddFluentValidationServices()
            .AddCorsServices()
            .AddRateLimitingServices()
            .AddRouting(options => options.LowercaseUrls = true)
            .AddMappers()
            .AddExceptionHandlers()
            .AddSignalRServices()
            .AddTransient<IStripeEventConstructor, StripeEventConstructor>()
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return builder.Services;
    }
}