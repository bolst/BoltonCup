using System.Reflection;
using MudBlazor;

namespace BoltonCup.Auth.Components.Shared.AutoEditForm;

public record FieldMetadata
{
    public required PropertyInfo PropertyInfo { get; init; }
    public required Type Type { get; init; }
    public required string Label { get; init; }
    public required InputType InputType { get; init; }
    public required bool IsReadOnly { get; init; }
}