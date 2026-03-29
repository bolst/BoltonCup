using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using MudBlazor;

namespace BoltonCup.Common.Components;

public record FieldContext
{
    public PropertyInfo PropertyInfo { get; }
    public Type Type { get; }
    public string Label { get; }
    public string? HelperText { get; }
    public InputType InputType { get; }
    public bool ReadOnly { get; }
    public bool IsRequired { get; }
    public string[]? AllowedValues { get; }

    public FieldContext(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
        Type = propertyInfo.PropertyType;
        Label = GetDisplayName(propertyInfo);
        HelperText = GetDescription(propertyInfo);
        InputType = GetInputType(propertyInfo);
        ReadOnly = GetReadOnly(propertyInfo);
        IsRequired = propertyInfo.GetCustomAttribute<RequiredAttribute>() is not null;
        AllowedValues = GetAllowedValues(propertyInfo);
    }
    
    private static string GetDisplayName(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttribute<DisplayAttribute>();
        return attr?.Name ?? prop.Name;
    }

    private static string? GetDescription(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description;
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
            DataType.PhoneNumber => InputType.Telephone,
            _ => InputType.Text
        };
    }

    private static bool GetReadOnly(PropertyInfo prop)
    {
        // only read-only if [ReadOnly(true)]
        var attr = prop.GetCustomAttribute<ReadOnlyAttribute>();
        return attr is not null && attr.IsReadOnly;
    }

    private static string[]? GetAllowedValues(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttribute<AllowedValuesAttribute>();
        if (attr?.Values == null)
            return null;

        return attr.Values.All(v => v is string or null) 
            ? attr.Values.Select(v => (string)v!).ToArray() 
            : null;
    }
}