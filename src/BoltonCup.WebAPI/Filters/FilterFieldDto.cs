namespace BoltonCup.WebAPI.Filters;

public sealed record FilterFieldDto
{
    public required string FieldName { get; init; }

    public required string Label { get; init; }

    public required FilterFieldType Type { get; init; }
    
    public bool Required { get; set; }
    
    public FilterOptionsSourceDto OptionsSource { get; init; }
}