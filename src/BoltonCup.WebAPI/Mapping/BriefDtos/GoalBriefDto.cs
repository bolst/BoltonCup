namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of a goal event.</summary>
public sealed record GoalBriefDto
{
    /// <summary>Gets or sets the goal ID.</summary>
    public required int Id { get; set; }
    /// <summary>Gets or sets the time remaining in the period when the goal was scored.</summary>
    public required TimeSpan TimeRemaining { get; set; }
    /// <summary>Gets or sets the period in which the goal was scored.</summary>
    public required int Period { get; set; }
    /// <summary>Gets or sets the ID of the team that scored the goal.</summary>
    public required int TeamId { get; set; }
    /// <summary>Gets or sets the player who scored the goal.</summary>
    public required PlayerBriefDto Scorer  { get; set; }
    /// <summary>Gets or sets the player credited with the primary assist.</summary>
    public required PlayerBriefDto? PrimaryAssist { get; set; }
    /// <summary>Gets or sets the player credited with the secondary assist.</summary>
    public required PlayerBriefDto? SecondaryAssist { get; set; }

    /// <summary>Gets the display label for the period.</summary>
    public string? PeriodLabel => Utilities.Mapping.TryGetPeriodName(Period);
    /// <summary>Gets the abbreviated label for the period.</summary>
    public string? PeriodAbbreviation => Utilities.Mapping.TryGetPeriodAbbreviation(Period);
    /// <summary>Gets the formatted time remaining as a mm:ss string.</summary>
    public string TimeString => TimeRemaining.ToString(@"mm\:ss");
}