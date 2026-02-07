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
        }
    };
}