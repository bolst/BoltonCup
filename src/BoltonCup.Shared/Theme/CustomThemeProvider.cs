using MudBlazor;

namespace BoltonCup.Shared.Data;

public class CustomThemeProvider
{
    public MudTheme Theme { get; }
    
    private readonly PaletteLight _defaultLight = new()
    {
        Primary = BCColors.Blue,
        Secondary = BCColors.Red,
        Tertiary = BCColors.Beige,
        Surface = BCColors.LightGray,
        Black = BCColors.Black,
        AppbarText = BCColors.White,
        AppbarBackground = BCColors.Blue,
        DrawerBackground = BCColors.White,
        GrayLight = BCColors.Gray,
        GrayLighter = BCColors.LightGray,
        Dark = BCColors.Navy,
        DarkLighten = BCColors.LightNavy,
        DarkDarken = BCColors.DarkNavy,
        DarkContrastText = BCColors.Beige
    };
    
    private readonly PaletteDark _defaultDark = new()
    {
        Primary = BCColors.Blue,
        PrimaryContrastText = BCColors.White,
        Secondary = BCColors.Red,
        Tertiary = BCColors.Beige,
        TertiaryContrastText = BCColors.Navy,
        Surface = BCColors.LightNavy,
        Black = BCColors.Black,
        AppbarText = BCColors.White,
        AppbarBackground = BCColors.DarkNavy,
        DrawerBackground = BCColors.DarkNavy,
        DrawerText = BCColors.White,
        Background = BCColors.Navy,
        GrayLight = BCColors.Gray,
        GrayLighter =  BCColors.LightGray,
        Dark = BCColors.Navy,
        DarkLighten = BCColors.LightNavy,
        DarkDarken = BCColors.DarkNavy,
        DarkContrastText = BCColors.LightBeige,
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