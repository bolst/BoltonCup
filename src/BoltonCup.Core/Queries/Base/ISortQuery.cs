namespace BoltonCup.Core.Queries.Base;


public interface ISortQuery
{
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
}