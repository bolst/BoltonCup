namespace BoltonCup.Core.Queries.Base;


public interface ISortQuery
{
    string? SortBy { get; set; }
    bool Descending { get; set; }
}