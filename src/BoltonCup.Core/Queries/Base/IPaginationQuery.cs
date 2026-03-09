namespace BoltonCup.Core.Queries.Base;


public interface IPaginationQuery
{
    public int Page  { get; set; }
    public int Size  { get; set; }
}