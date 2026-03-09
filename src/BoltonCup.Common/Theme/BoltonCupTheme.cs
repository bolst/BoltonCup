using MudBlazor;

namespace BoltonCup.Common.Theme;

public class BoltonCupTheme
{
    public MudTheme MudTheme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#0B2551",
            Secondary = "#EB443A",
            Tertiary = "#DDCBA4",
            AppbarBackground = "#191A21",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#4A6EAD",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#F0665E", 
            Tertiary = "#E5D8BC", 
            Surface = "#0E1116",
            DrawerBackground = "#16191F",
            DrawerText = "#FFFFFF",
            DrawerIcon = "#FFFFFF",
            AppbarBackground = "#020409",
            AppbarText = "#FFFFFF",
            Background = "#020409",
            TextPrimary = "#CDCDCD",
        }
    };
}