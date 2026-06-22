namespace BoltonCup.Core;

public interface ITournamentPlayerInfoService
{
    Task<TournamentPlayerInfoContext> GetAsync(int tournamentId, int accountId, CancellationToken cancellationToken = default);
    Task UpsertAsync(UpsertTournamentPlayerInfoCommand command, CancellationToken cancellationToken = default);
}

public record UpsertTournamentPlayerInfoCommand(
    int TournamentId,
    int AccountId,
    IReadOnlyList<GameAvailabilitySelection> GameAvailability,
    SongRequestSelection? Song);

public record GameAvailabilitySelection(int GameId, GameAvailability Availability);

public record SongRequestSelection(string TrackId, string Name, string Artist, string? AlbumArtUrl);

public record TournamentPlayerInfoContext(TournamentPlayerInfo? Info, IReadOnlyList<Game> TeamGames);
