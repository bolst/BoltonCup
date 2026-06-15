namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of a penalty event.</summary>
public sealed record PenaltyBriefDto
{
    /// <summary>Gets or sets the penalty ID.</summary>
    public required int Id { get; set; }
    /// <summary>Gets or sets the time remaining in the period when the penalty occurred.</summary>
    public required TimeSpan TimeRemaining { get; set; }
    /// <summary>Gets or sets the period in which the penalty occurred.</summary>
    public required int Period { get; set; }
    /// <summary>Gets or sets the ID of the team that received the penalty.</summary>
    public required int TeamId { get; set; }
    /// <summary>Gets or sets the player who received the penalty.</summary>
    public required PlayerBriefDto Player  { get; set; }
    /// <summary>Gets or sets the name of the infraction.</summary>
    public required string Infraction { get; set; }
    /// <summary>Gets or sets the duration of the penalty in minutes.</summary>
    public required int DurationMins { get; set; }

    /// <summary>Gets the display label for the period.</summary>
    public string? PeriodLabel => Utilities.Mapping.TryGetPeriodName(Period);
    /// <summary>Gets the abbreviated label for the period.</summary>
    public string? PeriodAbbreviation => Utilities.Mapping.TryGetPeriodAbbreviation(Period);
    /// <summary>Gets the formatted time remaining as a mm:ss string.</summary>
    public string TimeString => TimeRemaining.ToString(@"mm\:ss");
}