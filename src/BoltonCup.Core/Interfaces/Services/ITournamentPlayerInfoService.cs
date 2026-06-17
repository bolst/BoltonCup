namespace BoltonCup.Core;

public interface ITournamentPlayerInfoService
{
    Task<TournamentPlayerInfoContext> GetAsync(int tournamentId, int accountId, CancellationToken cancellationToken = default);
    Task UpsertAsync(UpsertTournamentPlayerInfoCommand command, CancellationToken cancellationToken = default);
}

public record UpsertTournamentPlayerInfoCommand(int TournamentId, int AccountId, string? Payload);

public record TournamentPlayerInfoContext(TournamentPlayerInfo? Info, IReadOnlyList<Game> TeamGames);
