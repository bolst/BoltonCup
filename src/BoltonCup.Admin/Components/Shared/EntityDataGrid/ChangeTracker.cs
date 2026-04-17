using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

        void TrackNode(EntityEntryGraphNode node)
        {
            var entry = node.Entry;
            
            // check if is root item
            if (NewItems.Contains(entry.Entity))
            {
                entry.State = EntityState.Added;
                return;
            }
            if (EditItems.Contains(entry.Entity))
            {
                entry.State = EntityState.Modified;
                return;
            }
            
            // check for new foreign entity
            if (!entry.IsKeySet)
            {
                entry.State = EntityState.Added;
                return;
            }
            
            // check for collisions
            var pk = entry.Metadata.FindPrimaryKey();
            if (pk is not null)
            {
                var keyValues = pk.Properties.Select(p => entry.Property(p.Name).CurrentValue).ToArray();
                
                // check if we are already tracking this entity
                var existingTracked = dbContext.ChangeTracker.Entries()
                    .FirstOrDefault(e => e.Metadata.ClrType == entry.Metadata.ClrType &&
                                         e.Metadata.FindPrimaryKey()?.Properties
                                             .Select(p => e.Property(p.Name).CurrentValue)
                                             .SequenceEqual(keyValues) == true);

                if (existingTracked is not null)
                {
                    // handle collision
                    if (existingTracked != entry.Entity)
                    {
                        if (node.InboundNavigation is { IsCollection: false })
                        {
                            var propertyInfo = node.InboundNavigation.PropertyInfo;
                            if (propertyInfo is { CanWrite: true })
                            {
                                propertyInfo.SetValue(node.SourceEntry!.Entity, existingTracked.Entity);
                            }
                        }
                    }

                    return;
                }
            }

            entry.State = EntityState.Unchanged;
        }
        
        // new and edit items
        foreach (var item in NewItems.Concat(EditItems))
        {
            dbContext.ChangeTracker.TrackGraph(item, TrackNode);
        }
        
        // delete items
        if (DeleteItems.Count > 0)
        {
            dbSet.RemoveRange(DeleteItems);
        }
        
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