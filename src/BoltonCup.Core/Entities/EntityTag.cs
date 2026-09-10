namespace BoltonCup.Core;

/// <summary>
/// A tag row linking a subject entity to exactly one target. Concrete tag types get their own
/// table; a database check constraint enforces that exactly one target FK is non-null.
/// </summary>
public abstract class EntityTag : EntityBase
{
    public int Id { get; set; }

    /// <summary>FK to the tagged subject. Mapped to a per-table column, e.g. <c>highlight_id</c>.</summary>
    public int SubjectId { get; set; }

    public int? GameId { get; set; }
    public int? PlayerId { get; set; }
    public int? TeamId { get; set; }
    public int? TournamentId { get; set; }
    public int? LabelId { get; set; }

    public Game? Game { get; set; }
    public Player? Player { get; set; }
    public Team? Team { get; set; }
    public Tournament? Tournament { get; set; }
    public TagLabel? Label { get; set; }
}

public abstract class EntityTag<TSubject> : EntityTag
    where TSubject : class
{
    public TSubject Subject { get; set; } = null!;
}
