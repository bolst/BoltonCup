using BoltonCup.Core;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides music track search for song-request pickers.</summary>
[Route("api/music")]
public class MusicController(
    IMusicSearchService _musicSearchService,
    IMapper _mapper
) : BoltonCupControllerBase
{
    const int MinSearchLength = 3;

    /// <summary>Searches for tracks matching the given query.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpGet("tracks")]
    public async Task<ActionResult<IReadOnlyList<MusicTrackDto>>> SearchTracks([FromQuery] string q, [FromQuery] int limit = 8)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < MinSearchLength)
        {
            return Ok(Array.Empty<MusicTrackDto>());
        }

        var clampedLimit = Math.Clamp(limit, 1, 20);
        var tracks = await _musicSearchService.SearchTracksAsync(q, clampedLimit);
        return Ok(_mapper.ToDto(tracks));
    }
}
