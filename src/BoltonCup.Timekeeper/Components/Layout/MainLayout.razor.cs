using BoltonCup.Common.Theme;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BoltonCup.Timekeeper.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] 
    private BoltonCupTheme Theme { get; set; } = null!;

    private MudTheme GetTheme()
    {
        var theme = Theme.MudTheme;
        theme.PaletteLight.DrawerBackground = theme.PaletteLight.Tertiary;
        theme.PaletteLight.DrawerText = theme.PaletteLight.Primary;
        return theme;
    }
}
