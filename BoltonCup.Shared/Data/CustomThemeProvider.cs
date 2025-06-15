using MudBlazor;

namespace BoltonCup.Shared.Data;

public class CustomThemeProvider
{
    public MudTheme Theme { get; }
    
    private readonly PaletteLight _defaultLight = new()
    {
        Primary = "#3364BE",
        Secondary = "#EB443A",
        Tertiary = "#DDCBA4",
        Black = "#141414",
        AppbarText = "#ffffff",
        AppbarBackground = "#3364BE",
        DrawerBackground = "#eddfc0",
        GrayLight = "#e8e8e8",
        GrayLighter = "#f9f9f9",
        Dark = "#151721",
        DarkContrastText = "#DDCBA4"
    };
    
    private readonly PaletteDark _defaultDark = new()
    {
        Primary = "#DDCBA4",
        PrimaryContrastText = "#191a21",
        Secondary = "#EB443A",
        Surface = "#232636",
        Black = "#ffffff",
        Tertiary = "#3364BE",
        TertiaryContrastText = "#ffffff",
        AppbarText = "#ffffff",
        AppbarBackground = "#0f1014",
        DrawerBackground = "#232636",
        DrawerText = "#ffffff",
        Background = "#191a21",
        GrayLight = "#e8e8e8",
        GrayLighter = "#f9f9f9",
        Dark = "#e0e0e0",
        DarkContrastText = "#2e2e2e"
    };

    public CustomThemeProvider()
    {
        Theme = new()
        {
            PaletteLight = _defaultLight,
            PaletteDark = _defaultDark,
            LayoutProperties = new LayoutProperties()
        };
    }

    public CustomThemeProvider(LayoutProperties layoutProperties)
    {
        Theme = new MudTheme()
        {
            PaletteLight = _defaultLight,
            PaletteDark = _defaultDark,
            LayoutProperties = layoutProperties
        };
    }
}