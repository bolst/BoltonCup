namespace BoltonCup.Core;

/// <summary>An entity's ID (0 when unsaved) and the slug it would like to use.</summary>
public sealed record SlugCandidate(int Id, string Slug);