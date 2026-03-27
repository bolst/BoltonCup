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
    private HashSet<T> _selectedItems;
    private MudDataGrid<T> _dataGrid = null!;
    private string? _search;
    private readonly ParameterState<string?> _searchState;

    private readonly List<Func<IQueryable<T>, IQueryable<T>>> _includeQueries = [];

    public EntityDataGrid()
    {
        _cts = new CancellationTokenSource();
        _changeTracker = new ChangeTracker<T>(Comparer);
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
    
    [Inject]
    public IDbContextFactory<BoltonCupDbContext> DbContextFactory { get; set; } = null!;

    [Parameter]
    public RenderFragment? Columns { get; set; }

    [Parameter]
    public EventCallback<ChangeTracker<T>> OnSave { get; set; }
    
    [Parameter]
    public EventCallback<ChangeTracker<T>> OnRevert { get; set; }

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
    
    [Parameter]
    public Expression<Func<T, string?>>? SearchBy { get; set; }
    
    [Parameter]
    public Func<GridData<T>, GridState<T>>? ServerFunc { get; set; }
    
    public Task NotifyItemChangedAsync(T item) => _dataGrid.CommittedItemChanges.InvokeAsync(item);

    private async Task<GridData<T>> ServerFuncWrapper(GridState<T> state)
    { 
        await _cts.CancelAsync();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        var cancellationToken = _cts.Token;
        
        try
        {
            var sortDefinition = state.SortDefinitions.FirstOrDefault();
            
            await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
            var dbSet = dbContext
                .Set<T>()
                .AsNoTracking();

            dbSet = _includeQueries.Aggregate(dbSet,
                (current, query) => query(current)
            );

            if (SearchBy is not null)
                dbSet = dbSet.WhereContains(SearchBy, _search);
            
            var data = await dbSet
                .ToPagedListAsync(new QueryBase
                {
                    Page = state.Page + 1,
                    Size = state.PageSize,
                    SortBy = sortDefinition?.SortBy,
                    Descending = sortDefinition?.Descending ?? false,
                }, cancellationToken);
            
            var items = _changeTracker.NewItems.Concat(data.Items);
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

    private void OnComparerChange(ParameterChangedEventArgs<IEqualityComparer<T>> args)
    {
        _changeTracker = new ChangeTracker<T>(args.Value);
        _selectedItems = new HashSet<T>(args.Value);
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
        _changeTracker.TrackEdit(item);
    }
    
    public async Task SaveChangesAsync()
    {
        await OnSave.InvokeAsync(_changeTracker);
        await _changeTracker.SaveChangesAsync(DbContextFactory);
        await _dataGrid.ReloadServerData();
    }

    public async Task RevertChangesAsync()
    {
        await OnRevert.InvokeAsync(_changeTracker);
        _changeTracker.Clear();
        await _dataGrid.ReloadServerData();
    }

    public void DeleteSelectedItems()
    {
        _changeTracker.TrackDeletes(_selectedItems);
        _selectedItems.Clear();
    }

    public async Task AddNewItem()
    {
        if (NewItemFunc is null)
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
        return HidePagerContent ? _noPagerHeight : _height;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
    
}