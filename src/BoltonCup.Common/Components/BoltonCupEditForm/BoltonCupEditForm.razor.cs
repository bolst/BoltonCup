using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BoltonCup.Common.Components;

public partial class BoltonCupEditForm<T> : ComponentBase where T : class, new()
{
    private readonly List<FieldContext> _fields = [];

    private bool _isSubmitting;

    private EditContext? _editContext;

    [Parameter]
    [EditorRequired]
    public required T Model { get; set; }
    
    [Parameter]
    public EventCallback<EditContext> OnValidSubmit { get; set; }

    [Parameter]
    public RenderFragment ActionsContent { get; set; } = null!;
    
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public Margin Margin { get; set; } = Margin.None;
    
    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public Variant ReadOnlyVariant { get; set; } = Variant.Text;

    [Parameter]
    public Variant Variant { get; set; } = Variant.Outlined;
    

    protected override void OnInitialized()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            _fields.Add(new FieldContext(propertyInfo));
        }
    }

    protected override void OnParametersSet()
    {
        _editContext ??= new EditContext(Model);
    }

    private async Task HandleValidSubmitAsync()
    {
        _isSubmitting = true;
        try
        {
            await OnValidSubmit.InvokeAsync(_editContext);
        }
        finally
        {
            _isSubmitting = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnValueChanged(FieldContext field, object? value)
    {
        field.PropertyInfo.SetValue(Model, value);
    }

    // creates the "() => Model.Property" expression
    private Expression<Func<TProperty>> CreateValidationExpression<TProperty>(FieldContext field)
    {
        var constant = Expression.Constant(Model);
        var propertyAccess = Expression.Property(constant, field.PropertyInfo);
        return Expression.Lambda<Func<TProperty>>(propertyAccess);
    }
}