namespace BoltonCup.Shared.Data;


public static class StringExtensions
{
    public static string FixedLengthWithEllipsis(this string source, int length) => source.Length <= length ? source : $"{source.Substring(0, length)}...";
}