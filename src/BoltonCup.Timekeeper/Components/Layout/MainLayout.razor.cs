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
        theme.PaletteLight.DrawerBackground = theme.PaletteDark.DrawerBackground;
        theme.PaletteLight.DrawerIcon = theme.PaletteDark.DrawerIcon;
        theme.PaletteLight.DrawerText = theme.PaletteDark.DrawerText;
        return theme;
    }
}
