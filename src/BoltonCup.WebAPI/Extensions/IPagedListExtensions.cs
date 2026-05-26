using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;

namespace BoltonCup.WebAPI;

/// <summary>Extension methods for <see cref="IPagedList{T}"/>.</summary>
public static class IPagedListExtensions
{
    /// <summary>Projects each element of a paged list into a new form, preserving pagination metadata.</summary>
    public static IPagedList<TResult> ProjectTo<TResult, TSource>(this IPagedList<TSource> source, Func<TSource, TResult> selector)
    {
        var projection = source.Items.Select(selector).ToList();
        return new PagedList<TResult>(projection, source.Total, source.Page, source.Size);
    }
}