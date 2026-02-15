using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using BoltonCup.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BoltonCup.Admin.Components.Shared;

public abstract partial class EntityColumn<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T, TProperty>
    : Column<T>
where T : EntityBase
where TProperty : EntityBase
{
    private Expression<Func<T, TProperty?>>? _lastAssignedProperty;
    private Func<T, TProperty?>? _compiledExpression;

    protected abstract RenderFragment EntityEditTemplate(TProperty? context);
    
    [Parameter]
    [EditorRequired]
    public required Expression<Func<T, TProperty?>> Property { get; set; } = Expression.Lambda<Func<T, TProperty?>>(Expression.Default(typeof(TProperty)), Expression.Parameter(typeof(T)));
    

    protected override void OnInitialized()
    {
        base.OnInitialized();
        EditTemplate = context =>
        {
            var item = _compiledExpression?.Invoke(context.Item);
            return EntityEditTemplate(item);
        };
    }
    
    protected override void OnParametersSet()
    {
        if (_lastAssignedProperty != Property)
        {
            _lastAssignedProperty = Property;
            _compiledExpression = Property.Compile();
        }
        
        Title ??= typeof(TProperty).Name;
    }
    
    protected override object? CellContent(T item)
        => _compiledExpression!(item);
    
    protected override object? PropertyFunc(T item)
    {
        _compiledExpression ??= Property.Compile();
        return _compiledExpression(item);
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
    
    protected override void SetProperty(object? item, object? value)
    {
        var expression = Property.Body;
        
        // Only MemberExpression is supported, MemberExpression access members like 'x.y' is accessing the member 'y'
        if (expression is not MemberExpression memberExpression) 
            return;
        if (memberExpression.Member is not PropertyInfo propertyInfo) 
            return;
        
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
    }

}

