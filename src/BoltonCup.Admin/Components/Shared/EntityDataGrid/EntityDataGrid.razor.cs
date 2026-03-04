using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.State;
using MudBlazor.Utilities;

namespace BoltonCup.Admin.Components.Shared;

[CascadingTypeParameter(nameof(T))]
public partial class EntityDataGrid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> 
    : ComponentBaseWithState, IDisposable
{
    private CancellationTokenSource _cts;
    private string _itemChangedStyle = new StyleBuilder()
        .AddStyle("background-color", "var(--mud-palette-dark-lighten)")
        .AddStyle("background-image", "linear-gradient(135deg, hsla(0, 0%, 100%, 0.05) 25%, transparent 0, transparent 50%, hsla(0, 0%, 100%, 0.05) 0, hsla(0, 0%, 100%, 0.05) 75%, transparent 0, transparent)")
        .AddStyle("background-size", "20px 20px")
        .Build();
    private const string _height = "calc(100vh - 64px - 64px - var(--mud-appbar-height))";
    private const string _noPagerHeight = "calc(100vh - 64px - var(--mud-appbar-height))";
    private readonly int[] _pageSizeOptions = [15, 50, 100, 500];
    private HashSet<T> _changes;
    private HashSet<T> _deletedItems;
    private HashSet<T> _newItems;
    private HashSet<T> _selectedItems;
    private MudDataGrid<T> _dataGrid = null!;
    private bool _isDirty;
    private string? _search;
    private readonly ParameterState<string?> _searchState;

    public EntityDataGrid()
    {
        _cts = new CancellationTokenSource();
        _changes = new HashSet<T>(Comparer);
        _deletedItems = new HashSet<T>(Comparer);
        _newItems = new HashSet<T>(Comparer);
        _selectedItems = new HashSet<T>(Comparer);
        using var registerScope = CreateRegisterScope();
        _searchState = registerScope.RegisterParameter<string?>(nameof(Search))
            .WithParameter(() => Search)
            .WithEventCallback(() => SearchChanged)
            .WithChangeHandler(OnSearchChange);
        registerScope.RegisterParameter<IEqualityComparer<T>>(nameof(Comparer))
            .WithParameter(() => Comparer)
            .WithChangeHandler(OnComparerChange);
    }

    [Parameter]
    public RenderFragment? Columns { get; set; }

    [Parameter]
    public EventCallback<HashSet<T>> OnSave { get; set; }
    
    [Parameter]
    public EventCallback<HashSet<T>> OnRevert { get; set; }
    
    [Parameter]
    public Func<GridState<T>, CancellationToken, Task<GridData<T>>> ServerFunc { get; set; } = null!;

    [Parameter]
    public string? Search { get; set; }
    
    [Parameter]
    public EventCallback<string?> SearchChanged { get; set; }
    
    [Parameter]
    public IEqualityComparer<T> Comparer { get; set; } = EqualityComparer<T>.Default;

    [Parameter]
    public ResizeMode ColumnResizeMode { get; set; } = ResizeMode.Container;
    
    [Parameter]
    public bool HidePagerContent { get; set; }

    [Parameter]
    public bool Selectable { get; set; } = true;
    
    [Parameter]
    public Func<Task<T?>>? NewItemFunc { get; set; }
    
    public Task NotifyItemChangedAsync(T item) => _dataGrid.CommittedItemChanges.InvokeAsync(item);

    private async Task<GridData<T>> ServerFuncWrapper(GridState<T> state)
    { 
        await _cts.CancelAsync();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        try
        {
            var gridData = await ServerFunc(state, _cts.Token);
            var items = _newItems.Concat(gridData.Items);
            return new GridData<T>
            {
                Items = items,
                TotalItems = gridData.TotalItems,
            };
        }
        catch (Exception e)
        when (e is OperationCanceledException or ObjectDisposedException)
        {
            return new GridData<T>();
        }
    }
    
    private Task OnSearchChange(ParameterChangedEventArgs<string?> args)
    {
        _search = args.Value;
        return _dataGrid.ReloadServerData();
    }

    private void OnComparerChange(ParameterChangedEventArgs<IEqualityComparer<T>> args)
    {
        _changes = new HashSet<T>(args.Value);
    }

    private async Task SetSearchAsync(string search)
    {
        if (search.Equals(_search, StringComparison.OrdinalIgnoreCase))
            return;
        _search = search;
        await _dataGrid.ReloadServerData();
        await _searchState.SetValueAsync(search);
    }
    
    private void OnItemEdited(T item)
    {
        _changes.Add(item);
        _isDirty = true;
    }
    
    public async Task SaveChangesAsync()
    {
        await OnSave.InvokeAsync(_changes);
        _changes.Clear();
        _deletedItems.Clear();
        _newItems.Clear();
        _isDirty = false;
        await _dataGrid.ReloadServerData();
    }

    public async Task RevertChangesAsync()
    {
        await OnRevert.InvokeAsync(_changes);
        _changes.Clear();
        _deletedItems.Clear();
        _newItems.Clear();
        _isDirty = false;
        await _dataGrid.ReloadServerData();
    }

    public void DeleteSelectedItems()
    {
        _isDirty = true;
        _deletedItems.UnionWith(_selectedItems);
        _selectedItems.Clear();
    }

    public async Task AddNewItem()
    {
        if (NewItemFunc is null)
            return;
        var newItem = await NewItemFunc();
        if (newItem is not null)
        {
            _newItems.Add(newItem);
            _isDirty = true;
        }
    }
    
    private string RowStyleFunc(T item, int row)
    {
        if (_deletedItems.Contains(item))
            return "background-color: #FF000055";
        if (_changes.Contains(item))
            return _itemChangedStyle;
        if (_newItems.Contains(item))
            return "background-color: #88FFAA55";
        return string.Empty;
    }

    private string GetHeight()
    {
        return HidePagerContent ? _noPagerHeight : _height;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
    
}