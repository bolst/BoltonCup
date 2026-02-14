namespace BoltonCup.Core.Queries.Base;

public interface IQueryBase 
    : IPaginationQuery, ISortQuery 
{ }

public record QueryBase : IQueryBase
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 50;
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
}