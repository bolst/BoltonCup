namespace BoltonCup.Core.Exceptions;

public sealed class InvalidRosterException(IEnumerable<string> reasons)
    : BoltonCupException("Trade would create an invalid roster: " + string.Join(" ", reasons))
{
    public IReadOnlyList<string> Reasons { get; } = reasons.ToList();
}
