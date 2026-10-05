using System.ComponentModel.DataAnnotations;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of published albums.</summary>
public record GetAlbumsRequest : RequestBase
{
    /// <summary>When set, only albums tagged with a label of this name (case-insensitive).</summary>
    [StringLength(64)]
    public string? Label { get; set; }
}