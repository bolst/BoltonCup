namespace BoltonCup.WebAPI.Filters;

public sealed record FilterSchemaDto
{
    public List<FilterFieldDto> PrimaryFields { get; init; }= [];

    public List<FilterFieldDto> SecondaryFields { get; init; }= [];
}