using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;

namespace BoltonCup.WebAPI;

public static class IPagedListExtensions
{
    public static IPagedList<TResult> ProjectTo<TResult, TSource>(this IPagedList<TSource> source, Func<TSource, TResult> selector)
    {
        var projection = source.Items.Select(selector).ToList();
        return new PagedList<TResult>(projection, source.Total, source.Page, source.Size);
    }
}