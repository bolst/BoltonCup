using System.Linq.Expressions;
using BoltonCup.Core.Queries;

namespace BoltonCup.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, PaginationQueryBase query)
    {
        var skip = (query.Page - 1) * query.Size;
        return source.Skip(skip).Take(query.Size);
    }
    
    
    /// <summary>
    /// Filters a sequence of values based on a predicate if the specified condition is true.
    /// </summary>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to filter.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="condition">A function to determine whether the filtering should be applied.</param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <returns>
    /// An <see cref="IQueryable{TSource}"/> that contains elements from the input sequence that satisfy the condition specified by <paramref name="predicate"/> if <paramref name="condition"/> is true; otherwise, the original <paramref name="source"/>.
    /// </returns>
    public static IQueryable<TSource> ConditionalWhere<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate,
        Func<bool> condition) => source.ConditionalWhere(predicate, condition());
    
    
    /// <summary>
    /// Filters a sequence of values based on a predicate if the specified condition is true.
    /// </summary>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to filter.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="condition">Determines whether the filtering should be applied.</param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <returns>
    /// An <see cref="IQueryable{TSource}"/> that contains elements from the input sequence that satisfy the condition specified by <paramref name="predicate"/> if <paramref name="condition"/> is true; otherwise, the original <paramref name="source"/>.
    /// </returns>
    public static IQueryable<TSource> ConditionalWhere<TSource>(
        this IQueryable<TSource> source, 
        Expression<Func<TSource, bool>> predicate, 
        bool condition)
    {
        return condition ? source.Where(predicate) : source;
    }
}