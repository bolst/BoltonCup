namespace BoltonCup.WebAPI.Utilities;

// temporary solution
// when I decide to be more pragmatic I will delete this

/// <summary>Utility helpers for converting raw domain values to display strings.</summary>
public static class Mapping
{
    /// <summary>Returns the full period name for the given period number, or <see langword="null"/> if unrecognised.</summary>
    public static string? TryGetPeriodName(int period)
    {
        return period switch
        {
            1 => "1st Period",
            2 => "2nd Period",
            3 => "3rd Period",
            4 => "Overtime",
            5 => "Shootout",
            _ => null
        };
    }

    /// <summary>Returns the abbreviated period label for the given period number, or <see langword="null"/> if unrecognised.</summary>
    public static string? TryGetPeriodAbbreviation(int period)
    {
        return period switch
        {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            4 => "OT",
            5 => "SO",
            _ => null
        };
    }
}