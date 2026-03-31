using BoltonCup.Sdk;

namespace BoltonCup.Common.Components;

public record FilterValue
{
    public FilterOptionDto? Value { get; init; }
    
    public List<FilterOptionDto>? Values { get; init; }
    
    public DateTime? DateFrom { get; init; }
    
    public DateTime? DateTo { get; init; }

    
    public bool IsEmpty => Value is null 
                           && (Values is null || Values.Count == 0) 
                           && DateFrom is null 
                           && DateTo is null;
}