using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using BoltonCup.Auth.Components.Shared.AutoEditForm;

namespace BoltonCup.Auth.Components.Shared;

public partial class AutoEditForm<T> where T : class, new()
{
    private T? _model { get; set; }
    private readonly List<FieldMetadata> _fields = [];

    private bool _isSubmitting;
    
    [Parameter, EditorRequired]
    public required EditContext EditContext { get; set; }

    [Parameter]
    public EventCallback<EditContext> OnValidSubmit { get; set; }

    [Parameter]
    public RenderFragment ActionsContent { get; set; } = null!;

    protected override void OnInitialized()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            _fields.Add(new FieldMetadata
            {
                PropertyInfo = propertyInfo,
                Type = propertyInfo.PropertyType,
                Label = GetDisplayName(propertyInfo),
                InputType = GetInputType(propertyInfo)
            });
        }
    }

    protected override void OnParametersSet()
    {
        if (EditContext.Model is not T model)
            throw new InvalidOperationException($"EditContext model must be of type {typeof(T).Name}");
        
        if (_model != model)
        {
            _model = model;
        }
    }

    private async Task HandleValidSubmitAsync()
    {
        _isSubmitting = true;
        try
        {
            await OnValidSubmit.InvokeAsync(EditContext);
        }
        finally
        {
            _isSubmitting = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private static string GetDisplayName(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttribute<DisplayAttribute>();
        return attr?.Name ?? prop.Name;
    }

    private static InputType GetInputType(PropertyInfo prop)
    {
        // first check for [EmailAddress]
        if (prop.GetCustomAttribute<EmailAddressAttribute>() != null) 
            return InputType.Email;

        var dataType = prop.GetCustomAttribute<DataTypeAttribute>();
        return dataType?.DataType switch
        {
            DataType.EmailAddress => InputType.Email,
            DataType.Password => InputType.Password,
            _ => InputType.Text
        };
    }

    private void OnValueChanged(FieldMetadata field, object? value)
    {
        field.PropertyInfo.SetValue(_model, value);
    }

    // creates the "() => Model.Property" expression
    private Expression<Func<TProperty>> CreateValidationExpression<TProperty>(FieldMetadata field)
    {
        var constant = Expression.Constant(_model);
        var propertyAccess = Expression.Property(constant, field.PropertyInfo);
        return Expression.Lambda<Func<TProperty>>(propertyAccess);
    }
}