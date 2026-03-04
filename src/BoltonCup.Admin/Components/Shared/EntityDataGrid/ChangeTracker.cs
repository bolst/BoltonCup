namespace BoltonCup.Admin.Components.Shared;

public sealed class ChangeTracker<T>
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
        EditItems.Add(item);
        IsDirty = true;
    }

    public void TrackDelete(T item)
    {
        DeleteItems.Add(item);
        IsDirty = true;
    }
    
    public void TrackDeletes(IEnumerable<T> items)
    {
        DeleteItems.UnionWith(items);
        IsDirty = true;
    }

    public void TrackNew(T item)
    {
        NewItems.Add(item);
        IsDirty = true;
    }

    public void Clear()
    {
        EditItems.Clear();
        DeleteItems.Clear();
        NewItems.Clear();
        IsDirty = false;
    }
}