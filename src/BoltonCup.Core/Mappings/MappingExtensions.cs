namespace BoltonCup.Core.Mappings;

public static class MappingExtensions
{
    public static IQueryable<TResult> ProjectTo<TSource, TResult>(
        this IQueryable<TSource> source, 
        IMappable<TSource, TResult> map)
    {
        return source.Select(map.Projection);
    }    
}