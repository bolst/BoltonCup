namespace BoltonCup.WebAPI.Utilities;

// temporary solution
// when I decide to be more pragmatic I will delete this

public static class Mapping
{
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