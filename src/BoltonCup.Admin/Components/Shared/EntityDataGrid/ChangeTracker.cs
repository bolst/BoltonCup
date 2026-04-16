using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Admin.Components.Shared;

public sealed class ChangeTracker<T> 
    where T : class
{
    public HashSet<T> EditItems { get; private set; }
    public HashSet<T> DeleteItems { get; private set; }
    public HashSet<T> NewItems { get; private set; }

    public bool IsDirty => EditItems.Count > 0 || DeleteItems.Count > 0 || NewItems.Count > 0;

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
    }

    public void TrackDelete(T item)
    {
        EditItems.Remove(item);
        
        // db doesnt have this yet
        if (!NewItems.Remove(item))
        {
            DeleteItems.Add(item);
        }
    }
    
    public void TrackDeletes(IEnumerable<T> items)
    {
        var enumerable = items.ToHashSet();
        EditItems.ExceptWith(enumerable);
        DeleteItems.UnionWith(enumerable);
    }

    public void TrackNew(T item)
    {
        EditItems.Remove(item);
        NewItems.Add(item);
    }

    public async Task SaveChangesAsync(DbContext dbContext)
    {
        var dbSet = dbContext.Set<T>();
        
        // new items
        foreach (var newItem in NewItems)
        {
            dbContext.ChangeTracker.TrackGraph(newItem, node =>
            {
                // if entity already has an ID, we assume it exists
                node.Entry.State = node.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;
            });
        }
        
        // edit items
        foreach (var editItem in EditItems)
        {
            dbContext.ChangeTracker.TrackGraph(editItem, node =>
            {
                if (node.Entry.Entity == editItem)
                    node.Entry.State = EntityState.Modified;
                else
                    node.Entry.State = node.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;
            });
        }
        
        // delete items
        if (DeleteItems.Count > 0)
            dbSet.RemoveRange(DeleteItems);
        
        await dbContext.SaveChangesAsync();
        Clear();
    }

    public void Clear()
    {
        EditItems.Clear();
        DeleteItems.Clear();
        NewItems.Clear();
    }
}