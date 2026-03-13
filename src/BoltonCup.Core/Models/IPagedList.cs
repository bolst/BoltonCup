namespace BoltonCup.Core;

public interface IPagedList<out T>
{
    IReadOnlyList<T> Items { get; }
    int Total { get; }
    int Page { get; }
    int Size { get; }
}