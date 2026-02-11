using System.Linq.Dynamic.Core;
using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Infrastructure.Extensions;

public static class SortQueryExtensions
{
    public static IQueryable<TSource> ApplySorting<TSource>(
        this IQueryable<TSource> source, 
        ISortQuery query,
        Func<IQueryable<TSource>, IOrderedQueryable<TSource>>? fallback)
    {
        if (string.IsNullOrEmpty(query.SortBy))
        {
            return fallback is not null
                ? fallback(source)
                : source;
        }

        var direction = query.Descending ? "desc" : "asc";
        var expr = $"{query.SortBy} {direction}";
        return source.OrderBy(expr);
    }
}

