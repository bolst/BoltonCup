using System.Linq.Expressions;
using System.Reflection;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BoltonCup.Admin.Components.Shared;

public partial class ForeignColumn<T, TEntity> : Column<T>
    where T : EntityBase
    where TEntity : EntityBase
{

    private string _entityName;
    private Func<T, TEntity?>? _compiledExpression;
    private List<TEntity> _options = new();

    [CascadingParameter]
    public EntityDataGrid<T> ParentGrid { get; set; } = null!;

    [Inject]
    public IDbContextFactory<BoltonCupDbContext> DbContextFactory { get; set; } = null!;

    [Parameter, EditorRequired]
    public Expression<Func<T, TEntity?>> Property { get; set; } = null!;

    [Parameter]
    public Action<T, TEntity?>? PropertySetter { get; set; }
    
    [Parameter]
    public new IEqualityComparer<TEntity?>? EqualityComparer { get; set; }
    
    [Parameter]
    public Func<TEntity, string?>? ImageSrcFunc { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        EditTemplate = EntityEditTemplate;
        if (Property.Body is MemberExpression memberExpr)
        {
            if (string.IsNullOrEmpty(Title))
                Title = memberExpr.Member.Name;
            _entityName = memberExpr.Member.Name;
        }

        await LoadOptionsAsync();
    }
    
    public override string? PropertyName 
        => _entityName;

    protected override object? CellContent(T item)
    {
        _compiledExpression ??= Property.Compile();
        return _compiledExpression(item);
    }
    
    protected override object? PropertyFunc(T item)
    {
        _compiledExpression ??= Property.Compile();
        return _compiledExpression(item);
    }
    
    protected override Type PropertyType
        => typeof(TEntity);

    protected override void SetProperty(object? item, object? value)
    {
        var expression = Property.Body;
        
        // Only MemberExpression is supported, MemberExpression access members like 'x.y' is accessing the member 'y'
        if (expression is not MemberExpression memberExpression) 
            return;
        if (memberExpression.Member is not PropertyInfo propertyInfo) 
            return;
        
        var rootItem = item;
        item = RecursiveGetSubProperties(memberExpression, item);
        if (value == null)
        { 
            propertyInfo.SetValue(item, null);
        }
        else
        {
            var actualType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? PropertyType;
            propertyInfo.SetValue(item, Convert.ChangeType(value, actualType), null);
        }
        
        if (rootItem is T entity)
            InvokeAsync(() => ParentGrid.NotifyItemChangedAsync(entity));
    }
    
    private object? RecursiveGetSubProperties(MemberExpression memberExpression, object? item)
    {
        if (memberExpression.Expression is not MemberExpression
            {
                Member: PropertyInfo propertyInfo
            } subMemberExpress) 
            return item;
        
        var subObject = RecursiveGetSubProperties(subMemberExpress, item);
        return propertyInfo.GetValue(subObject) 
               ?? throw new NullReferenceException($"Unable to get property value, value of '{propertyInfo.Name}' is null in '{Property}'");
    }

    private async Task LoadOptionsAsync()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        // Grabs all records of the target foreign entity type
        _options = await dbContext
            .Set<TEntity>()
            .AsNoTracking()
            .ToListAsync();
    }

    private async Task OnValueChangedAsync(T item, TEntity? newValue)
    {
        if (PropertySetter is not null)
        {
            PropertySetter(item, newValue);
        }
        else
        {
            var propertyInfo = (Property.Body as MemberExpression)?.Member as PropertyInfo;
            propertyInfo?.SetValue(item, newValue);
        }
        await ParentGrid.NotifyItemChangedAsync(item);
    }
}