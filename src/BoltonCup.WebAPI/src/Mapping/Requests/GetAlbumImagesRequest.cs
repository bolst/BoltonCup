using System.ComponentModel.DataAnnotations;
using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of images carrying a given tag, across published albums.</summary>
public record GetAlbumImagesRequest : RequestBase
{
    /// <summary>Gets or sets the kind of entity the tag targets.</summary>
    [Required]
    [EnumDataType(typeof(TagTargetType))]
    public TagTargetType TagType { get; set; }

    /// <summary>Gets or sets the id of the tagged entity.</summary>
    [Required]
    public int TagTargetId { get; set; }
}