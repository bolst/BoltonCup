using System.Diagnostics.CodeAnalysis;
using BoltonCup.Core;
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
    private const string _height = "calc(100vh - 64px - 52px - var(--mud-appbar-height))";
    private const string _noPagerHeight = "calc(100vh - 64px - var(--mud-appbar-height))";
    private readonly int[] _pageSizeOptions = [15, 50, 100];
    private HashSet<T> _changes;
    private MudDataGrid<T> _dataGrid = null!;
    private bool _isDirty;
    private string? _search;
    private readonly ParameterState<string?> _searchState;
    private readonly ParameterState<IEqualityComparer<T>> _comparerState;

    public EntityDataGrid()
    {
        _cts = new CancellationTokenSource();
        _changes = new HashSet<T>(Comparer);
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
    
    public Task NotifyItemChangedAsync(T item) => _dataGrid.CommittedItemChanges.InvokeAsync(item);

    private Task<GridData<T>> ServerFuncWrapper(GridState<T> state)
    { 
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        try
        {
            return ServerFunc(state, _cts.Token);
        }
        catch (Exception e)
        when (e is OperationCanceledException or ObjectDisposedException)
        {
            return Task.FromResult(new GridData<T>());
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
        _isDirty = false;
        await _dataGrid.ReloadServerData();
    }

    public async Task RevertChangesAsync()
    {
        await OnRevert.InvokeAsync(_changes);
        _changes.Clear();
        _isDirty = false;
        await _dataGrid.ReloadServerData();
    }
    
    private string RowStyleFunc(T item, int row)
    {
        return !_changes.Contains(item) 
            ? string.Empty 
            : _itemChangedStyle;
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