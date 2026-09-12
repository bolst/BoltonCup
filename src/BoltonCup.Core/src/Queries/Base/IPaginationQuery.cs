namespace BoltonCup.Core.Queries.Base;


public interface IPaginationQuery
{
    int Page { get; set; }
    int Size { get; set; }
}