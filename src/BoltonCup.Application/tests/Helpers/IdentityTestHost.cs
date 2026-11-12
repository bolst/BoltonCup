using BoltonCup.Application.Services;
using BoltonCup.Core;
using BoltonCup.Integrations.Email;
using BoltonCup.Persistence.Data;
using BoltonCup.Persistence.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BoltonCup.Application.Tests.Helpers;

/// <summary>
/// Real Identity (UserManager, SignInManager, token providers) over InMemory stores, configured with the app's
/// shared <see cref="BoltonCup.Persistence.ServiceCollectionExtensions.ConfigureIdentity"/> options.
/// </summary>
sealed class IdentityTestHost : IDisposable
{
    public const string Password = "Correct-Horse-1";

    readonly ServiceProvider _provider;
    readonly IServiceScope _scope;

    public MutableTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
    public Mock<IEmailer> Emailer { get; } = new();
    public Mock<IAccountService> AccountService { get; } = new();

    /// <summary>The last code sent to each address, by any of the code emails.</summary>
    public Dictionary<string, string> SentCodes { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <param name="configureServices">
    /// Runs after the defaults, so it can add an extra <see cref="IUserValidator{TUser}"/> or replace
    /// <see cref="UserManager{TUser}"/> with a subclass.
    /// </param>
    /// <param name="postgresConnectionString">
    /// Runs both contexts on this database (see <see cref="PostgresFixture"/>) with the real account service, instead of
    /// InMemory with <see cref="AccountService"/>.
    /// </param>
    public IdentityTestHost(Action<IServiceCollection>? configureServices = null, string? postgresConnectionString = null)
    {
        Emailer
            .Setup(e => e.SendPasswordResetCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((email, code) => SentCodes[email] = code)
            .Returns(Task.CompletedTask);
        Emailer
            .Setup(e => e.SendConfirmationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((email, code) => SentCodes[email] = code)
            .Returns(Task.CompletedTask);

        var databaseName = $"identity-{Guid.NewGuid()}";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddAuthentication().AddIdentityCookies();
        if (postgresConnectionString is null)
        {
            services.AddDbContext<AuthDbContext>(o => o.UseInMemoryDatabase($"{databaseName}-auth"));
            services.AddDbContext<BoltonCupDbContext>(o => o.UseInMemoryDatabase($"{databaseName}-app"));
            services.AddSingleton(AccountService.Object);
        }
        else
        {
            services.AddDbContext<AuthDbContext>(o => o.UseNpgsql(postgresConnectionString));
            services.AddDbContext<BoltonCupDbContext>(o => o.UseNpgsql(postgresConnectionString));
            services.AddSingleton(Mock.Of<IStorageService>());
            services.AddSingleton(Mock.Of<IAssetKeyGenerator>());
            services.AddScoped<IAccountService, BoltonCup.Application.Services.AccountService>();
        }

        services
            .AddIdentityCore<BoltonCupUser>(BoltonCup.Persistence.ServiceCollectionExtensions.ConfigureIdentity)
            .AddRoles<IdentityRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<BoltonCupClaimsPrincipalFactory>();
        services.AddSingleton(Emailer.Object);
        services.AddScoped(sp => new CodeAttemptGuard(sp.GetRequiredService<UserManager<BoltonCupUser>>(), Clock, sp.GetRequiredService<ILogger<CodeAttemptGuard>>()));
        services.AddScoped<IUserService, UserService>();
        configureServices?.Invoke(services);

        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();

        // Password sign-in writes the auth cookie, which needs an HttpContext.
        SignInManager.Context = new DefaultHttpContext { RequestServices = _scope.ServiceProvider };
    }

    public IServiceProvider Services => _scope.ServiceProvider;

    /// <summary>A separate scope, for acting as a parallel request or reading what the store saved.</summary>
    public IServiceScope CreateScope() => _provider.CreateScope();
    public IUserService UserService => Services.GetRequiredService<IUserService>();
    public UserManager<BoltonCupUser> UserManager => Services.GetRequiredService<UserManager<BoltonCupUser>>();
    public SignInManager<BoltonCupUser> SignInManager => Services.GetRequiredService<SignInManager<BoltonCupUser>>();
    public BoltonCupDbContext Db => Services.GetRequiredService<BoltonCupDbContext>();

    /// <param name="userName">Defaults to the email, as registration does.</param>
    public async Task<BoltonCupUser> CreateUserAsync(string email, bool emailConfirmed = true, string? userName = null)
    {
        var user = new BoltonCupUser
        {
            UserName = userName ?? email,
            Email = email,
            EmailConfirmed = emailConfirmed,
        };
        var result = await UserManager.CreateAsync(user, Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return user;
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }
}

sealed class MutableTimeProvider(DateTimeOffset start) : TimeProvider
{
    DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}