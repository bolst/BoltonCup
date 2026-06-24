using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using BoltonCup.Auth.Attributes;
using MudBlazor;

namespace BoltonCup.Auth.Components.Shared.AutoEditForm;

public record FieldMetadata(PropertyInfo PropertyInfo)
{
    public readonly Type Type = PropertyInfo.PropertyType;

    public readonly string Label = GetDisplayName(PropertyInfo);

    public readonly InputType InputType = GetInputType(PropertyInfo);

    public readonly bool ReadOnly = GetReadOnly(PropertyInfo);

    public readonly bool AutoFocus = GetAutoFocus(PropertyInfo);

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

    private static bool GetReadOnly(PropertyInfo prop)
    {
        // only read-only if [ReadOnly(true)]
        var attr = prop.GetCustomAttribute<ReadOnlyAttribute>();
        return attr is not null && attr.IsReadOnly;
    }

    private static bool GetAutoFocus(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttribute<AutoFocusAttribute>();
        return attr is { AutoFocus: true };
    }
}