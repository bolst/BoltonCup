namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing one day of scores and the index of every game day.</summary>
public sealed record GameScoresDto
{
    /// <summary>Gets the Eastern-time day shown.</summary>
    public required DateOnly Date { get; init; }
    /// <summary>Gets the closest earlier game day, if any.</summary>
    public DateOnly? PreviousDate { get; init; }
    /// <summary>Gets the closest later game day, if any.</summary>
    public DateOnly? NextDate { get; init; }
    /// <summary>Gets every game day across all tournaments, in date order, one entry per tournament per day.</summary>
    public required List<GameDayDto> Days { get; init; } = [];
    /// <summary>Gets the games on the day shown, in start-time order.</summary>
    public required List<GameScoreDto> Games { get; init; } = [];
}