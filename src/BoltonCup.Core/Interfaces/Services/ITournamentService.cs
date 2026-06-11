namespace BoltonCup.Core;

public interface ITournamentService
{
    Task UpdateLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateBackgroundImageAsync(int tournamentId, string tempKey, CancellationToken cancellationToken = default);
}