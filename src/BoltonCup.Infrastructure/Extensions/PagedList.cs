using BoltonCup.Core;
using BoltonCup.Core.Queries.Base;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Extensions;


public static class PagedListQueryExtensions
{

    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> source, 
        IPaginationQuery query, 
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken: cancellationToken);
        var skip = (query.Page - 1) * query.Size;
        var items = await source.Skip(skip).Take(query.Size).ToListAsync(cancellationToken);
        return new PagedList<T>(items, totalCount, query.Page, query.Size);
    }
}


public class PagedList<T>(IReadOnlyList<T> items, int total, int page, int size)
    : IPagedList<T>
{
    public IReadOnlyList<T> Items { get; } = items;
    public int Total { get; } = total;
    public int Page { get; } = page;
    public int Size { get; } = size;
}