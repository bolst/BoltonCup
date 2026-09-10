namespace BoltonCup.Core;

public enum TagTargetType
{
    Game,
    Player,
    Team,
    Tournament,
    Label,
}

/// <summary>
/// Everything that varies per tag target, declared once. The model configuration, the check
/// constraint, the services and the Admin UI all derive from <see cref="All"/>, so adding a
/// target means adding one entry here plus its foreign key and navigation on <see cref="EntityTag"/>.
/// </summary>
public sealed record TagTarget(
    TagTargetType Type,
    Type ClrType,
    string Navigation,
    string ForeignKey,
    string Column,
    bool Selectable,
    Func<EntityTag, int?> GetId,
    Action<EntityTag, int?> SetId,
    Func<EntityTag, object?> GetEntity);

public static class TagTargets
{
    public static readonly IReadOnlyList<TagTarget> All =
    [
        new(TagTargetType.Game, typeof(Game), nameof(EntityTag.Game), nameof(EntityTag.GameId), "game_id", true,
            t => t.GameId, (t, v) => t.GameId = v, t => t.Game),
        new(TagTargetType.Player, typeof(Player), nameof(EntityTag.Player), nameof(EntityTag.PlayerId), "player_id", true,
            t => t.PlayerId, (t, v) => t.PlayerId = v, t => t.Player),
        new(TagTargetType.Team, typeof(Team), nameof(EntityTag.Team), nameof(EntityTag.TeamId), "team_id", true,
            t => t.TeamId, (t, v) => t.TeamId = v, t => t.Team),
        new(TagTargetType.Tournament, typeof(Tournament), nameof(EntityTag.Tournament), nameof(EntityTag.TournamentId), "tournament_id", true,
            t => t.TournamentId, (t, v) => t.TournamentId = v, t => t.Tournament),
        // Not selectable: labels are created by typing free text, never picked from the type list.
        new(TagTargetType.Label, typeof(TagLabel), nameof(EntityTag.Label), nameof(EntityTag.LabelId), "label_id", false,
            t => t.LabelId, (t, v) => t.LabelId = v, t => t.Label),
    ];

    static readonly Dictionary<TagTargetType, TagTarget> ByType = All.ToDictionary(t => t.Type);

    /// <summary>The exclusive-arc constraint, generated so it can never drift from <see cref="All"/>.</summary>
    public static string CheckConstraintSql
        => $"num_nonnulls({string.Join(", ", All.Select(t => t.Column))}) = 1";

    public static IEnumerable<TagTargetType> Selectable
        => All.Where(t => t.Selectable).Select(t => t.Type);

    public static TagTarget Of(TagTargetType type) => ByType.TryGetValue(type, out var target)
        ? target
        : throw new ArgumentOutOfRangeException(nameof(type), type, "No tag target is declared for this type.");

    /// <summary>
    /// The type a tag row targets. Null when the row does not carry exactly one target, which the
    /// check constraint forbids — returning null rather than the first match keeps corruption visible.
    /// </summary>
    public static TagTargetType? GetTargetType(EntityTag tag)
    {
        TagTargetType? found = null;
        foreach (var target in All)
        {
            if (target.GetId(tag) is null)
            {
                continue;
            }

            if (found is not null)
            {
                return null;
            }

            found = target.Type;
        }

        return found;
    }

    public static int? GetTargetId(EntityTag tag, TagTargetType type) => Of(type).GetId(tag);

    public static object? GetTargetEntity(EntityTag tag, TagTargetType type) => Of(type).GetEntity(tag);

    /// <summary>Points the tag at one target, clearing the others so the arc stays exclusive.</summary>
    public static void SetTargetId(EntityTag tag, TagTargetType type, int targetId)
    {
        Of(type);
        foreach (var target in All)
        {
            target.SetId(tag, target.Type == type ? targetId : null);
        }
    }
}
