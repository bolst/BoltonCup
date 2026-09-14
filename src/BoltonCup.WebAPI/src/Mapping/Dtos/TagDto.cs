using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>A tag on a subject entity, resolved to a display name.</summary>
public record TagDto
{
    /// <summary>Gets the unique identifier of the tag row.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the kind of entity this tag points at.</summary>
    public required TagTargetType Type { get; init; }
    /// <summary>Gets the identifier of the target entity.</summary>
    public required int TargetId { get; init; }
    /// <summary>Gets the display name of the target, e.g. a team name or label text.</summary>
    public required string Name { get; init; }
}
