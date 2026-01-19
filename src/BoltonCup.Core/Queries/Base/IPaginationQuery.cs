namespace BoltonCup.Core.Queries.Base;


public interface IPaginationQuery
{
    public int Page  { get; set; }
    public int Size  { get; set; }
}


public record DefaultPaginationQuery : IPaginationQuery
{
    public int Page { get; set; } = 1;
    public virtual int Size { get; set; } = 50;
}