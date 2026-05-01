using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;

namespace BoltonCup.WebClient.Components.Shared;

public partial class River : ComponentBase
{
    /// <summary>
    /// The content to be rendered inside the carousel.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Determines how long (in seconds) it takes for the carousel to complete one full loop.
    /// Lower numbers mean a faster river.
    /// </summary>
    [Parameter]
    public double AnimationDuration { get; set; } = 30.0;

    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public string? Style { get; set; }
    
    private string Classname => new CssBuilder()
        .AddClass("fluid-carousel-container")
        .AddClass(Class)
        .Build();
    
    private string Stylename => new StyleBuilder()
        .AddStyle("--animation-speed", $"{AnimationDuration}s")
        .AddStyle(Style)
        .Build();
}
