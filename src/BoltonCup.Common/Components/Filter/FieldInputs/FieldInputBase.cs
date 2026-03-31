using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.State;

namespace BoltonCup.Common.Components;

public abstract class FieldInputBase : ComponentBaseWithState
{
    private readonly ParameterState<FilterValue?> _valueState;
    
    [Parameter]
    public List<Sdk.FilterOptionDto> Options { get; set; } = [];

    [Parameter]
    public FilterValue? Value { get; set; }
    
    [Parameter]
    public EventCallback<FilterValue?> ValueChanged { get; set; }


    protected FieldInputBase()
    {
        using var registerScope = CreateRegisterScope();
        _valueState = registerScope.RegisterParameter<FilterValue?>(nameof(Value))
            .WithParameter(() => Value)
            .WithEventCallback(() => ValueChanged)
            .WithChangeHandler(OnValueChange);
    }
    
    private async Task OnValueChange(ParameterChangedEventArgs<FilterValue?> args)
    {
        await OnValueChangedAsync(args.Value);
        await InvokeAsync(StateHasChanged);
    }
    
    protected async Task SetValueAsync(FilterValue? value)
    {
        await OnValueChangedAsync(value);
        await _valueState.SetValueAsync(value);
    }

    protected virtual Task OnValueChangedAsync(FilterValue? value)
        => Task.CompletedTask;
    
}