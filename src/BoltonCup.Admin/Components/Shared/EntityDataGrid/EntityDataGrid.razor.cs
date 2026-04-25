using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using BoltonCup.Admin.Extensions;
using BoltonCup.Core.Queries.Base;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.State;
using MudBlazor.Utilities;
using System.Linq.Dynamic.Core;

namespace BoltonCup.Admin.Components.Shared;

[CascadingTypeParameter(nameof(T))]
public partial class EntityDataGrid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> 
    : ComponentBaseWithState, IDisposable
    where T : class
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
    private ChangeTracker<T> _changeTracker;
    private MudDataGrid<T> _dataGrid = null!;
    private string? _search;
    private readonly ParameterState<string?> _searchState;
    private readonly ParameterState<HashSet<T>> _selectedItemsState;

    private readonly List<Func<IQueryable<T>, IQueryable<T>>> _includeQueries = [];

    public EntityDataGrid()
    {
        _cts = new CancellationTokenSource();
        _changeTracker = new ChangeTracker<T>(Comparer);
        Selection = new HashSet<T>(Comparer);
        SelectedItems = new HashSet<T>(Comparer);
        using var registerScope = CreateRegisterScope();
        _searchState = registerScope.RegisterParameter<string?>(nameof(Search))
            .WithParameter(() => Search)
            .WithEventCallback(() => SearchChanged)
            .WithChangeHandler(OnSearchChange);
        _selectedItemsState = registerScope.RegisterParameter<HashSet<T>>(nameof(SelectedItems))
            .WithParameter(() => SelectedItems)
            .WithEventCallback(() => SelectedItemsChanged)
            .WithChangeHandler(OnSelectedItemsChanged);
    }
    
    [Inject]
    public IDbContextFactory<BoltonCupDbContext> DbContextFactory { get; set; } = null!;

    [Parameter]
    public RenderFragment? Columns { get; set; }
    
    [Parameter]
    public RenderFragment? ActionMenu { get; set; }
    
    [Parameter]
    public RenderFragment? ToolbarContent { get; set; }
    
    [Parameter]
    public RenderFragment<CellContext<T>>? ChildRowContent { get; set; }

    [Parameter]
    public EventCallback<ChangeTracker<T>> OnSave { get; set; }
    
    [Parameter]
    public EventCallback<ChangeTracker<T>> OnRevert { get; set; }

    [Parameter]
    public string? Search { get; set; }
    
    [Parameter]
    public EventCallback<string?> SearchChanged { get; set; }

    [Parameter]
    public HashSet<T> SelectedItems { get; set; }
    
    [Parameter]
    public EventCallback<HashSet<T>> SelectedItemsChanged { get; set; }
    
    [Parameter]
    public HashSet<T> Selection { get; set; }

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
    
    [Parameter]
    public Expression<Func<T, string?>>? SearchBy { get; set; }
    
    [Parameter]
    public bool ReadOnly { get; set; }
    
    [Parameter]
    public Func<CancellationToken, Task<DbContext>>? DbContextFunc { get; set; }

    [Parameter]
    public Func<IQueryable<T>, IQueryable<T>>? Include { get; set; }
    
    [Parameter]
    public Expression<Func<T, bool>>? Filter { get; set; }
    
    [Parameter]
    public bool NoDeleting { get; set; }

    [Parameter]
    public string? Height { get; set; }
    
    [Parameter]
    public EventCallback<DbContext> OnServerReload { get; set; }

    public async Task NotifyItemChangedAsync(T item)
    {
        if (_dataGrid.CommittedItemChanges is not null)
        {
            await _dataGrid.CommittedItemChanges.Invoke(item);
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task<DbContext> CreateDbContext(CancellationToken cancellationToken = default)
    {
        if (DbContextFunc is not null)
            return await DbContextFunc(cancellationToken);
        return await DbContextFactory.CreateDbContextAsync(cancellationToken);
    }

    private async Task<GridData<T>> ServerFuncWrapper(GridState<T> state, CancellationToken cancellationToken)
    {
        Selection.Clear();
        await SetSelectedItemsAsync(Selection);
        
        try
        {
            await using var dbContext = await CreateDbContext(cancellationToken);
            var dbSet = dbContext
                .Set<T>()
                .AsNoTracking();

            if (Include is not null)
                dbSet = Include(dbSet);
            
            dbSet = _includeQueries.Aggregate(dbSet,
                (current, query) => query(current).AsNoTracking()
            );

            if (Filter is not null)
                dbSet = dbSet.Where(Filter);

            if (SearchBy is not null)
                dbSet = dbSet.WhereContains(SearchBy, _search);

            var sortExpression = string.Join(", ", state.SortDefinitions.Select(d => $"{d.SortBy} {(d.Descending ? "desc" : "asc")}"));
            if (!string.IsNullOrEmpty(sortExpression))
                dbSet = dbSet.OrderBy(sortExpression);
            
            var query = new QueryBase
            {
                Page = state.Page + 1,
                Size = state.PageSize,
            };
            
            var data = await dbSet
                .ToPagedListAsync(query, cancellationToken);
            var items = _changeTracker.NewItems.Concat(data.Items);
            
            await OnServerReload.InvokeAsync(dbContext);
            
            return new GridData<T>
            {
                Items = items,
                TotalItems = data.Total,
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

    private void OnSelectedItemsChanged(ParameterChangedEventArgs<HashSet<T>> args)
    {
        Selection.Clear();
        Selection.UnionWith(args.Value);
    }

    private Task SetSelectedItemsAsync(HashSet<T>? items)
    {
        items ??= new HashSet<T>(Comparer);
        Selection.Clear();
        Selection.UnionWith(items);
        return _selectedItemsState.SetValueAsync(new HashSet<T>(items, Comparer));
    }

    private async Task SetSearchAsync(string search)
    {
        if (search.Equals(_search, StringComparison.OrdinalIgnoreCase))
            return;
        _search = search;
        await _dataGrid.ReloadServerData();
        await _searchState.SetValueAsync(search);
    }
    
    private Task<DataGridEditFormAction> OnItemEdited(T item)
    {
        _changeTracker.TrackEdit(item);
        return Task.FromResult(DataGridEditFormAction.Close);
    }
    
    public async Task SaveChangesAsync()
    {
        if (ReadOnly)
            return;
        await using var dbContext = await CreateDbContext();
        await OnSave.InvokeAsync(_changeTracker);
        await _changeTracker.SaveChangesAsync(dbContext);
        await _dataGrid.ReloadServerData();
    }

    public async Task RevertChangesAsync()
    {
        await OnRevert.InvokeAsync(_changeTracker);
        _changeTracker.Clear();
        await _dataGrid.ReloadServerData();
    }

    public async Task DeleteSelectedItems()
    {
        if (ReadOnly)
            return;
        _changeTracker.TrackDeletes(Selection);
        Selection.Clear();
        await SetSelectedItemsAsync(Selection);
    }

    public async Task AddNewItem()
    {
        if (NewItemFunc is null || ReadOnly)
            return;
        var newItem = await NewItemFunc();
        if (newItem is not null)
        {
            _changeTracker.TrackNew(newItem);
        }
    }

    public void RegisterInclude(Func<IQueryable<T>, IQueryable<T>> includeQuery)
    {
        _includeQueries.Add(includeQuery);
    }
    
    public Task ReloadAsync()
        => _dataGrid.ReloadServerData();

    public Task ExtendSortAsync(string field, SortDirection direction, Func<T, object?> sortFunc)
        => _dataGrid.ExtendSortAsync(field, direction, sortFunc);
    
    private string RowStyleFunc(T item, int row)
    {
        if (_changeTracker.DeleteItems.Contains(item))
            return "background-color: #FF000055";
        if (_changeTracker.EditItems.Contains(item))
            return _itemChangedStyle;
        if (_changeTracker.NewItems.Contains(item))
            return "background-color: #88FFAA55";
        return string.Empty;
    }

    private string GetHeight()
    {
        if (!string.IsNullOrEmpty(Height))
            return Height;
        return HidePagerContent ? _noPagerHeight : _height;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
    
}