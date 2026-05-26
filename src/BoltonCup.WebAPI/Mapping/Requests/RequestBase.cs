using System.ComponentModel.DataAnnotations;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Base class for paged, sortable list requests.</summary>
public record RequestBase
{
    /// <summary>Gets or sets the 1-based page number to retrieve.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>Gets or sets the number of items per page.</summary>
    [Range(1, 100)]
    public int Size { get; set; } = 50;

    /// <summary>Gets or sets the field name to sort results by.</summary>
    public string? SortBy { get; set; }

    /// <summary>Gets or sets whether to sort results in descending order.</summary>
    public bool Descending { get; set; }
}