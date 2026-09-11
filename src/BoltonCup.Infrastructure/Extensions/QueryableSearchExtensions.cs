using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Extensions;

public static class QueryableSearchExtensions
{
    public static IQueryable<T> WhereContains<T>(
        this IQueryable<T> query,
        Expression<Func<T, string?>> selector,
        string? searchTerm
    )
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like",
            [typeof(DbFunctions), typeof(string), typeof(string)]);

        var lowerProperty = Expression.Call(selector.Body, toLowerMethod!);

        // The column is lowered below, so the pattern must be too, and LIKE metacharacters in
        // user input are escaped rather than treated as wildcards.
        var escaped = searchTerm.ToLower()
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
        var pattern = $"%{escaped}%";
        var patternConstant = Expression.Constant(pattern);

        var functionsProperty = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        var likeCall = Expression.Call(null, likeMethod!, functionsProperty, lowerProperty, patternConstant);
        var lambda = Expression.Lambda<Func<T, bool>>(likeCall, selector.Parameters);

        return query.Where(lambda);
    }
}