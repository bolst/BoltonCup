using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;

namespace BoltonCup.WebClient.Components.Shared;

public partial class RiverItem : ComponentBase
{
    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    string Classname => new CssBuilder()
        .AddClass("fluid-carousel-item")
        .AddClass(Class)
        .Build();

    string Stylename => new StyleBuilder()
        .AddStyle(Style)
        .Build();
}