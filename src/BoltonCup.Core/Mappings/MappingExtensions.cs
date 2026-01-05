namespace BoltonCup.Core.Mappings;

public static class MappingExtensions
{
    public static IQueryable<TResult> ProjectTo<TSource, TResult>(this IQueryable<TSource> source) 
        where TResult : IMappable<TSource, TResult>
    {
        return source.Select(TResult.Projection);
    }    
}