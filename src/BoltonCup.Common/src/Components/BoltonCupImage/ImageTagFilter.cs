namespace BoltonCup.Common.Components;

/// <summary>Identifies which tagged entity's published gallery images <see cref="EditImageDialog"/> offers to pick from.</summary>
public record ImageTagFilter(Sdk.TagTargetType Type, int TargetId);