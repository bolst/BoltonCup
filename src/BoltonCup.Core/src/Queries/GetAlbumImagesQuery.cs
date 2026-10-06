using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

/// <summary>Query for images in published albums carrying a given tag.</summary>
public sealed record GetAlbumImagesQuery : QueryBase
{
    /// <summary>The kind of entity the tag targets.</summary>
    public TagTargetType TagType { get; set; }

    /// <summary>The id of the tagged entity.</summary>
    public int TargetId { get; set; }
}