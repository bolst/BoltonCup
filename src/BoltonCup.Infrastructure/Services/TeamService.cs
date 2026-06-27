using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly BoltonCupDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IAssetKeyGenerator _assetKeyGenerator;
    private readonly IMusicLibraryService _music;

    public TeamService(BoltonCupDbContext dbContext, IStorageService storageService, IAssetKeyGenerator assetKeyGenerator, IMusicLibraryService music)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _assetKeyGenerator = assetKeyGenerator;
        _music = music;
    }
    
    public Task UpdateLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _storageService.UpdateAssetAsync<Team>(
            _dbContext,
            _assetKeyGenerator,
            t => t.Id == teamId,
            t => t.Logo,
            tempKey,
            teamId.ToString(),
            cancellationToken
        );
    }    
    
    public Task UpdateBannerAsync(int teamId, string tempKey, CancellationToken cancellationToken = default)
    {
        return _storageService.UpdateAssetAsync<Team>(
            _dbContext,
            _assetKeyGenerator,
            t => t.Id == teamId,
            t => t.Banner,
            tempKey,
            teamId.ToString(),
            cancellationToken
        );
    }

    public async Task UpdateSongsAsync(int teamId, MusicTrack? goalSong, MusicTrack? winSong, CancellationToken cancellationToken = default)
    {
        var team = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Team), teamId);

        team.GoalSongTrackId = await ResolveTrackIdAsync(team, goalSong, cancellationToken);
        team.WinSongTrackId = await ResolveTrackIdAsync(team, winSong, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // A picked track is registered in the tournament's music library (so the fetcher downloads it) and the
    // team points at it; clearing the pick returns null and leaves any existing track row untouched.
    private async Task<int?> ResolveTrackIdAsync(Team team, MusicTrack? song, CancellationToken cancellationToken)
    {
        if (song is null)
            return null;

        if (team.TournamentId is not { } tournamentId)
            throw new InvalidOperationException($"Team {team.Id} has no tournament; cannot register a song.");

        return await _music.EnsureTrackAsync(tournamentId, song, MusicTrackSource.PlayerRequest, cancellationToken);
    }
}