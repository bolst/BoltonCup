using System.ComponentModel.DataAnnotations;

namespace BoltonCup.WebAPI.Mapping;

public record RequestBase
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    
    [Range(1, 100)]
    public int Size { get; set; } = 50;
    
    public string? SortBy { get; set; }
    
    public bool Descending { get; set; }
}