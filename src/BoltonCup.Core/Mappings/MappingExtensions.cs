namespace BoltonCup.Core.Mappings;

public static class MappingExtensions
{
    public static IQueryable<TResult> ProjectTo<TSource, TResult>(this IQueryable<TSource> source) 
        where TResult : IMappable<TSource, TResult>
    {
        return source.Select(TResult.Projection);
    }    
    
    public static Task<CollectionResult<TSource>> ToCollectionResultAsync<TSource>(
        this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CollectionResult<TSource>(source));
    }
}