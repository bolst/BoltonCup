namespace BoltonCup.Core.Queries;

public abstract record PaginationQueryBase
{
    public int Page { get; init; } = 1;
    public virtual int Limit { get; init; } = 50;
}