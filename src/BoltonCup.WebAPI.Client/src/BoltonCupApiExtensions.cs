namespace BoltonCup.Sdk;

/// <summary>
/// Hand-written helpers on top of the generated <see cref="IBoltonCupApi"/> client.
/// </summary>
public static class BoltonCupApiExtensions
{
    /// <summary>
    /// Finds the tournament with <paramref name="id"/>, or <c>null</c> when <paramref name="id"/> is null or unknown.
    /// </summary>
    public static async Task<TournamentDto?> FindTournamentAsync(this IBoltonCupApi api, int? id, CancellationToken cancellationToken = default)
    {
        if (id is null)
        {
            return null;
        }
        var tournaments = await api.GetTournamentsAsync(cancellationToken: cancellationToken);
        return tournaments.Items.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// Finds the teams of tournament <paramref name="tournamentId"/> whose IDs appear in <paramref name="ids"/>.
    /// IDs of teams outside that tournament are dropped.
    /// </summary>
    public static async Task<IReadOnlyList<TeamDto>> FindTeamsAsync(this IBoltonCupApi api, int tournamentId, IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var wanted = ids.ToHashSet();
        if (wanted.Count == 0)
        {
            return [];
        }
        var teams = await api.GetTeamsAsync(tournamentId: tournamentId, cancellationToken: cancellationToken);
        return teams.Items
            .Where(x => wanted.Contains(x.Id))
            .ToList();
    }
}