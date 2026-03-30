namespace BoltonCup.WebAPI.Filters;

public sealed record FilterOptionsSourceDto
{
    public required FilterOptionsSourceType SourceType { get; init; }
    
    public string? EndpointPath { get; init; }

    public List<FilterOptionDto> StaticOptions { get; init; } = [];
}