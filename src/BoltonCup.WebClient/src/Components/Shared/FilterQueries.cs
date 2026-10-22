namespace BoltonCup.WebClient.Components.Shared;

/// <summary>
/// Filter IDs for <see cref="ScheduleDataGrid"/>, as carried in the page URL.
/// </summary>
public record ScheduleFilterQuery(int? TournamentId, int? TeamId);

/// <summary>
/// Filter IDs for <see cref="SkaterStatsDataGrid"/> and <see cref="GoalieStatsDataGrid"/>, as carried in the page URL.
/// </summary>
public record StatsFilterQuery(int? TournamentId, IReadOnlyList<int> TeamIds, string? Position)
{
    public static StatsFilterQuery Empty { get; } = new StatsFilterQuery(null, [], null);

    /// <summary>
    /// Compares by value; the record's own equality compares <see cref="TeamIds"/> by reference.
    /// </summary>
    public bool Matches(StatsFilterQuery other)
        => TournamentId == other.TournamentId
            && Position == other.Position
            && TeamIds.Order().SequenceEqual(other.TeamIds.Order());
}