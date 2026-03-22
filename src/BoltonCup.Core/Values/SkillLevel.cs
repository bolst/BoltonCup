namespace BoltonCup.Core.Values;

public static class SkillLevel
{
    public const string HouseLeague = "House league";
    public const string AA = "A/AA";
    public const string AAA = "AAA";
    public const string JrC = "Jr. C";
    public const string JrB = "Jr. B";
    public const string JrA = "Jr. A or higher";

    // A handy list to use inside our validator
    public static readonly IReadOnlyList<string> All = 
    [
        HouseLeague, AA, AAA, JrC, JrB, JrA
    ];
}