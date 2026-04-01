using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BoltonCup.Common.Components;

public abstract class FilterFieldBase<T> 
    : ComponentBaseWithState
{
    
    [CascadingParameter]
    public FilterToolbarContext<T>? ToolbarContext { get; set; }
    
    [Parameter]
    public T? Value { get; set; }
    
    [Parameter]
    public EventCallback<T?> ValueChanged { get; set; }
    
    [Parameter]
    public bool Disabled { get; set; }

    protected T? ContextValue
        => ToolbarContext is not null ? ToolbarContext.Value : Value;

    protected EventCallback<T?> ContextValueChanged =>
        ToolbarContext?.ValueChanged ?? ValueChanged;
}