using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Admin.Components.Shared;

public sealed class ChangeTracker<T> 
    where T : class
{
    public HashSet<T> EditItems { get; private set; }
    public HashSet<T> DeleteItems { get; private set; }
    public HashSet<T> NewItems { get; private set; }
    public bool IsDirty { get; private set; }

    public ChangeTracker(IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        EditItems = new HashSet<T>(comparer);
        DeleteItems = new HashSet<T>(comparer);
        NewItems = new HashSet<T>(comparer);
    }
    
    public void TrackEdit(T item)
    {
        if (DeleteItems.Contains(item) || NewItems.Contains(item))
            return;
        EditItems.Add(item);
        IsDirty = true;
    }

    public void TrackDelete(T item)
    {
        EditItems.Remove(item);
        DeleteItems.Add(item);
        IsDirty = true;
    }
    
    public void TrackDeletes(IEnumerable<T> items)
    {
        var enumerable = items.ToHashSet();
        EditItems.ExceptWith(enumerable);
        DeleteItems.UnionWith(enumerable);
        IsDirty = true;
    }

    public void TrackNew(T item)
    {
        EditItems.Remove(item);
        NewItems.Add(item);
        IsDirty = true;
    }

    public async Task SaveChangesAsync(IDbContextFactory<BoltonCupDbContext> dbContextFactory)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var dbSet = dbContext.Set<T>();
        dbSet.UpdateRange(EditItems);
        dbSet.RemoveRange(DeleteItems);
        dbSet.AddRange(NewItems);
        await dbContext.SaveChangesAsync();
        Clear();
    }

    public void Clear()
    {
        EditItems.Clear();
        DeleteItems.Clear();
        NewItems.Clear();
        IsDirty = false;
    }
}