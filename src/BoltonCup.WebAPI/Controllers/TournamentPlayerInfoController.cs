using BoltonCup.Core;
using BoltonCup.Shared;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages the authenticated user's pre-tournament player info (game availability and song request).</summary>
[Route("api/tournaments/{id:int}/player-info")]
public class TournamentPlayerInfoController(
    ITournamentPlayerInfoService _playerInfoService,
    IMapper _mapper
) : BoltonCupControllerBase
{
    /// <summary>Gets the authenticated user's player info and the games of their assigned team.</summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<TournamentPlayerInfoDto>> GetMyTournamentPlayerInfo(int id)
    {
        var accountId = User.GetAccountId();
        var context = await _playerInfoService.GetAsync(id, accountId);
        return Ok(_mapper.ToDto(context));
    }

    /// <summary>Creates or updates the authenticated user's player info for a tournament.</summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UpdateMyTournamentPlayerInfo(int id, [FromBody] UpdateTournamentPlayerInfoRequest data)
    {
        var accountId = User.GetAccountId();
        var command = new UpsertTournamentPlayerInfoCommand(
            id,
            accountId,
            data.GameAvailability.Select(a => new GameAvailabilitySelection(a.GameId, a.Availability)).ToList(),
            data.Song is { } song
                ? new SongRequestSelection(song.Id, song.Name, song.Artist, song.AlbumArtUrl)
                : null);
        await _playerInfoService.UpsertAsync(command);
        return Ok();
    }
}
