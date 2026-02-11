namespace BoltonCup.Core;


public class CollectionResult<T>(IEnumerable<T> items)
{
    public IEnumerable<T> Items { get; set; } = items;

    public CollectionResult() : this([])
    {
    }
}