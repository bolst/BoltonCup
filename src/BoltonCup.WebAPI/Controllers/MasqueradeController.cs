using System.Security.Claims;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Infrastructure.Services;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Admin-only impersonation: lets an admin assume another user's identity and return to their own.</summary>
[Route("/api/admin/masquerade")]
[Tags("Masquerade")]
public class MasqueradeController(
    UserManager<BoltonCupUser> _userManager,
    SignInManager<BoltonCupUser> _signInManager,
    IMasqueradeService _masqueradeService,
    ILogger<MasqueradeController> _logger
    ) : BoltonCupControllerBase
{
    /// <summary>Searches users an admin can masquerade as, by name or email.</summary>
    [Authorize(Roles = Admin)]
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<MasqueradeUserDto>>> SearchMasqueradeUsers([FromQuery] string? query, CancellationToken cancellationToken)
    {
        var users = await _masqueradeService.SearchAsync(query, cancellationToken: cancellationToken);
        return Ok(users
            .Select(u => new MasqueradeUserDto(u.UserId, u.AccountId, u.Name, u.Email))
            .ToList());
    }

    /// <summary>Starts masquerading as the specified user. Records the current admin so it can be restored.</summary>
    [Authorize(Roles = Admin)]
    [HttpPost]
    public async Task<ActionResult> Masquerade([FromBody] MasqueradeRequest request)
    {
        if (await _userManager.FindByIdAsync(request.UserId) is not { } target)
        {
            return NotFound();
        }

        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var adminUserName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "";

        var markerClaims = new[]
        {
            new Claim(BoltonCupClaimTypes.OriginalUserId, adminUserId),
            new Claim(BoltonCupClaimTypes.OriginalUserName, adminUserName),
        };

        await _signInManager.SignInWithClaimsAsync(target, isPersistent: false, markerClaims);
        _logger.LogInformation("Admin {AdminUserId} ({AdminUserName}) started masquerading as user {TargetUserId}", adminUserId, adminUserName, target.Id);
        return Ok();
    }

    /// <summary>Stops masquerading and restores the original admin identity.</summary>
    [Authorize]
    [HttpPost("stop")]
    public async Task<ActionResult> StopMasquerade()
    {
        var originalUserId = User.FindFirstValue(BoltonCupClaimTypes.OriginalUserId);
        if (string.IsNullOrEmpty(originalUserId))
        {
            return BadRequest("Not currently masquerading.");
        }

        var maskedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (await _userManager.FindByIdAsync(originalUserId) is not { } admin)
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        await _signInManager.SignInAsync(admin, isPersistent: true);
        _logger.LogInformation("Admin {AdminUserId} stopped masquerading (was user {TargetUserId})", originalUserId, maskedUserId);
        return Ok();
    }
}

/// <summary>Request payload for starting a masquerade session.</summary>
public record MasqueradeRequest(string UserId);
