using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages a tournament's uploaded music library (admin only).</summary>
[Route("api/tournaments/{id:int}/music")]
public class TournamentMusicController(
    IMusicLibraryService _music,
    IMapper _mapper
) : BoltonCupControllerBase
{
    /// <summary>Lists the tournament's music library.</summary>
    [Authorize(Roles = Admin)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MusicLibraryTrackDto>>> GetLibrary(int id)
    {
        var tracks = await _music.GetLibraryAsync(id);
        return Ok(_mapper.ToDtoList(tracks));
    }

    /// <summary>Player song requests in this tournament that have no matching uploaded file.</summary>
    [Authorize(Roles = Admin)]
    [HttpGet("missing")]
    public async Task<ActionResult<IReadOnlyList<MissingSongRequestDto>>> GetMissing(int id)
    {
        var missing = await _music.GetMissingRequestsAsync(id);
        return Ok(_mapper.ToDtoList(missing));
    }

    /// <summary>The tournament's music download queue (pending, downloaded, and cancelled items).</summary>
    [Authorize(Roles = Admin)]
    [HttpGet("queue")]
    public async Task<ActionResult<IReadOnlyList<MusicQueueItemDto>>> GetQueue(int id)
    {
        var queue = await _music.GetQueueAsync(id);
        return Ok(_mapper.ToDtoList(queue));
    }

    /// <summary>Adds an uploaded audio file to the library.</summary>
    [Authorize(Roles = Admin)]
    [HttpPost]
    public async Task<ActionResult<MusicLibraryTrackDto>> AddTrack(int id, [FromBody] AddMusicTrackRequest request)
    {
        var track = await _music.AddTrackAsync(_mapper.ToCommand(id, request));
        return Ok(_mapper.ToDto(track));
    }

    /// <summary>Updates a library track's metadata or base-pool flag.</summary>
    [Authorize(Roles = Admin)]
    [HttpPut("{trackId:int}")]
    public async Task<IActionResult> UpdateTrack(int id, int trackId, [FromBody] UpdateMusicTrackRequest request)
    {
        await _music.UpdateTrackAsync(_mapper.ToCommand(id, trackId, request));
        return Ok();
    }

    /// <summary>Removes a library track.</summary>
    [Authorize(Roles = Admin)]
    [HttpDelete("{trackId:int}")]
    public async Task<IActionResult> DeleteTrack(int id, int trackId)
    {
        await _music.DeleteTrackAsync(id, trackId);
        return Ok();
    }
}