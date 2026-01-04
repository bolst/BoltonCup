namespace BoltonCup.Core.Queries;


public interface IPaginationQuery
{
    int Page  { get; set; }
    int Size  { get; set; }
}


public record DefaultPaginationQuery : IPaginationQuery
{
    public int Page { get; set; } = 1;
    public virtual int Size { get; set; } = 50;
}