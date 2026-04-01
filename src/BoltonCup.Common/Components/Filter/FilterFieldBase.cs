using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BoltonCup.Common.Components;

public abstract class FilterFieldBase<T> 
    : ComponentBaseWithState
    where T : class
{
    [CascadingParameter]
    public FilterToolbarContext<T>? ToolbarContext { get; set; }
    
    [Parameter]
    public T? Value { get; set; }
    
    [Parameter]
    public EventCallback<T?> ValueChanged { get; set; }

    public T? ContextValue
        => ToolbarContext?.Value ?? Value;

    protected EventCallback<T?> ContextValueChanged =>
        ToolbarContext?.ValueChanged ?? ValueChanged;
}