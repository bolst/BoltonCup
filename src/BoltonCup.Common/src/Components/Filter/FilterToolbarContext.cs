using Microsoft.AspNetCore.Components;

namespace BoltonCup.Common.Components;

public record FilterToolbarContext<T>(string? Label, T? Value, EventCallback<T?> ValueChanged, bool Disabled);