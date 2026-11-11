using System.Text.Json;
using BoltonCup.Application.Exceptions;
using BoltonCup.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Application.Services;

enum CodePurpose
{
    PasswordReset,
    EmailConfirmation
}

// Caps anonymous code checks per user without touching Identity lockout, so wrong guesses never block password login.
class CodeAttemptGuard(UserManager<BoltonCupUser> _userManager, TimeProvider _timeProvider, ILogger<CodeAttemptGuard> _logger)
{
    public const int MaxAttempts = 10;
    public static readonly TimeSpan Window = TimeSpan.FromHours(1);

    const string LoginProvider = "BoltonCup";

    /// <summary>Records an attempt before the code is checked; throws when the window is full or the record cannot be saved.</summary>
    public async Task ConsumeAttemptAsync(BoltonCupUser user, CodePurpose purpose)
    {
        var now = _timeProvider.GetUtcNow();
        var window = await ReadWindowAsync(user, purpose);
        if (window is null || now - window.WindowStart >= Window)
        {
            window = new AttemptWindow(0, now);
        }

        if (window.Failures >= MaxAttempts)
        {
            throw new TooManyAttemptsException();
        }

        var updated = window with { Failures = window.Failures + 1 };
        IdentityResult result;
        try
        {
            result = await _userManager.SetAuthenticationTokenAsync(user, LoginProvider, TokenName(purpose), JsonSerializer.Serialize(updated));
        }
        catch (DbUpdateException)
        {
            // Parallel first attempts race to insert the same token row; the loser fails closed.
            throw new AttemptNotRecordedException();
        }

        if (!result.Succeeded)
        {
            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            if (!errorCodes.Contains(nameof(IdentityErrorDescriber.ConcurrencyFailure)))
            {
                _logger.LogWarning("Could not record code attempt for user {UserId}: {ErrorCodes}", user.Id, string.Join(", ", errorCodes));
            }

            throw new AttemptNotRecordedException();
        }
    }

    public Task ClearAsync(BoltonCupUser user, CodePurpose purpose) =>
        _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName(purpose));

    async Task<AttemptWindow?> ReadWindowAsync(BoltonCupUser user, CodePurpose purpose)
    {
        var json = await _userManager.GetAuthenticationTokenAsync(user, LoginProvider, TokenName(purpose));
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<AttemptWindow>(json);
    }

    static string TokenName(CodePurpose purpose) => $"{purpose}Attempts";

    record AttemptWindow(int Failures, DateTimeOffset WindowStart);
}