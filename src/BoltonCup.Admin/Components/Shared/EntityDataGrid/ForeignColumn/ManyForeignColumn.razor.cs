using System.Linq.Expressions;
using BoltonCup.Admin.Extensions;
using BoltonCup.Core;
using BoltonCup.Core.Queries.Base;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BoltonCup.Admin.Components.Shared;

/// <summary>Carries a multi-select foreign collection change for a grid row.</summary>
public record ManyForeignSelection<T, TEntity>(T Item, IReadOnlyList<TEntity> Selected)
    where T : EntityBase
    where TEntity : EntityBase;

/// <summary>A grid column that edits a many-to-many navigation collection via a searchable add + removable chips.</summary>
public partial class ManyForeignColumn<T, TEntity> : Column<T>
    where T : EntityBase
    where TEntity : EntityBase
{
    private string? _entityName;
    private Func<T, ICollection<TEntity>?>? _compiledExpression;
    private TEntity? _pendingAdd;
    private bool _pendingRegistration = true;

    [CascadingParameter]
    public EntityDataGrid<T> ParentGrid { get; set; } = null!;

    [Inject]
    public IDbContextFactory<BoltonCupDbContext> DbContextFactory { get; set; } = null!;

    [Parameter, EditorRequired]
    public Expression<Func<T, ICollection<TEntity>>> Property { get; set; } = null!;

    [Parameter]
    public IEqualityComparer<TEntity>? EqualityComparer { get; set; }

    [Parameter]
    public Func<TEntity, string?>? ImageSrcFunc { get; set; }

    /// <summary>Restricts the options offered for a given row (e.g. exclude accounts already managing in this tournament).</summary>
    [Parameter]
    public Func<T, Expression<Func<TEntity, bool>>>? Filter { get; set; }

    [Parameter]
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? Include { get; set; }

    [Parameter]
    public Expression<Func<TEntity, string?>>? SearchBy { get; set; }

    [Parameter]
    public EventCallback<ManyForeignSelection<T, TEntity>> OnSelectionChanged { get; set; }

    protected override Task OnInitializedAsync()
    {
        EditTemplate = EntityEditTemplate;
        if (Property.Body is MemberExpression memberExpr)
        {
            if (string.IsNullOrEmpty(Title))
                Title = memberExpr.Member.Name;
            _entityName = memberExpr.Member.Name;
        }

        return Task.CompletedTask;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (_pendingRegistration)
        {
            ParentGrid.RegisterInclude(query => query.Include(Property));
            _pendingRegistration = false;
        }
    }

    public override string? PropertyName
        => _entityName;

    private ICollection<TEntity> GetCollection(T item)
    {
        _compiledExpression ??= Property.Compile();
        return _compiledExpression(item) ?? [];
    }

    private IEqualityComparer<TEntity> EntityComparer
        => EqualityComparer ?? EqualityComparer<TEntity>.Default;

    protected override object? CellContent(T item)
        => string.Join(", ", GetCollection(item).Select(e => e.ToString()));

    protected override object? PropertyFunc(T item)
        => GetCollection(item);

    protected override Type PropertyType
        => typeof(ICollection<TEntity>);

    // Membership is persisted out-of-band via OnSelectionChanged; nothing to write back inline.
    protected override void SetProperty(object? item, object? value)
    {
    }

    private async Task<IEnumerable<TEntity>> SearchOptionsAsync(string? search, CellContext<T> context, CancellationToken token)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync(token);

        var dbSet = dbContext.Set<TEntity>().AsNoTracking();

        if (Include is not null)
            dbSet = Include(dbSet);

        var query = dbSet;
        if (Filter is not null)
            query = query.Where(Filter(context.Item));

        if (!string.IsNullOrEmpty(search) && SearchBy is not null)
            query = query.WhereContains(SearchBy, search);

        if (SearchBy is not null)
            query = query.OrderBy(SearchBy);

        var result = await query.ToPagedListAsync(new QueryBase { Size = 10 }, cancellationToken: token);

        // Hide options already selected on this row.
        var current = GetCollection(context.Item);
        return result.Items.Where(e => !current.Any(c => EntityComparer.Equals(c, e))).ToList();
    }

    private async Task AddAsync(T item, TEntity? value)
    {
        _pendingAdd = null;
        if (value is null)
            return;

        var collection = GetCollection(item);
        if (collection.Any(c => EntityComparer.Equals(c, value)))
            return;

        collection.Add(value);
        await NotifySelectionAsync(item, collection);
    }

    private async Task RemoveAsync(T item, TEntity value)
    {
        var collection = GetCollection(item);
        var existing = collection.FirstOrDefault(e => EntityComparer.Equals(e, value));
        if (existing is null)
            return;

        collection.Remove(existing);
        await NotifySelectionAsync(item, collection);
    }

    private Task NotifySelectionAsync(T item, ICollection<TEntity> collection)
        => OnSelectionChanged.InvokeAsync(new ManyForeignSelection<T, TEntity>(item, collection.ToList()));
}
