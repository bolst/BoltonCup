using BoltonCup.Core.Mappings;
using BoltonCup.Core.Queries.Base;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Extensions;


public static class PaginatedListQueryExtensions
{

    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source, 
        IPaginationQuery query, 
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken: cancellationToken);
        var skip = (query.Page - 1) * query.Size;
        var items = await source.Skip(skip).Take(query.Size).ToListAsync(cancellationToken);
        return new PaginatedList<T>(items, totalCount, query.Page, query.Size);
    }
}


public class PaginatedList<T> : CollectionResult<T>
{
    public int Total { get; }
    public int Page { get; }
    public int Size { get; }
    
    public PaginatedList(List<T> items, int total, int page, int size)
        : base(items)
    {
        Items = items;
        Total = total;
        Page = page;
        Size = size;
    }

}