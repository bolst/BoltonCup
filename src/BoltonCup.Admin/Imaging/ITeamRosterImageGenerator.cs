using BoltonCup.Common.Imaging;

namespace BoltonCup.Admin.Imaging;

public interface ITeamRosterImageGenerator
{
    /// <summary>Loads a team's players grouped by position, in the default render order.
    /// Used to seed the drag-to-reorder lists on the Image Generation page.</summary>
    Task<TeamRosterOptions> GetRosterAsync(int teamId, CancellationToken cancellationToken = default);

    /// <summary>Renders the roster PNG bytes only — no upload, no log row. Use for live preview.</summary>
    Task<byte[]> RenderAsync(int teamId, RosterColorway colorway, RosterOrdering? ordering = null,
        CancellationToken cancellationToken = default);

    /// <summary>Renders, uploads to storage, and records a log row.</summary>
    Task<GeneratedImageResult> GenerateAsync(int teamId, RosterColorway colorway, RosterOrdering? ordering = null,
        CancellationToken cancellationToken = default);
}

public sealed record GeneratedImageResult(byte[] Png, string StorageKey, string ContentType);

/// <summary>Ephemeral desired render order as player IDs per position. An empty/absent
/// list for a position falls back to the default sort (jersey number, then last name).</summary>
public sealed record RosterOrdering(
    IReadOnlyList<int> Forwards,
    IReadOnlyList<int> Defense,
    IReadOnlyList<int> Goalies)
{
    public static readonly RosterOrdering Empty = new([], [], []);
}

/// <summary>A single player as shown in the reorder UI.</summary>
public sealed record RosterPlayerOption(int PlayerId, string Name, int? JerseyNumber);

public sealed record TeamRosterOptions(
    IReadOnlyList<RosterPlayerOption> Forwards,
    IReadOnlyList<RosterPlayerOption> Defense,
    IReadOnlyList<RosterPlayerOption> Goalies);
