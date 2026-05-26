namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to record a penalty in a game.</summary>
public record CreatePenaltyRequest
{
    /// <summary>Gets or sets the ID of the penalized team.</summary>
    public int TeamId { get; set; }

    /// <summary>Gets or sets the period number.</summary>
    public int Period { get; set; }

    /// <summary>Gets or sets the period label (e.g. "1st", "OT").</summary>
    public string PeriodLabel { get; set; } = string.Empty;

    /// <summary>Gets or sets the time remaining in the period when the penalty was called.</summary>
    public TimeSpan PeriodTimeRemaining { get; set; }

    /// <summary>Gets or sets the ID of the penalized player.</summary>
    public int PlayerId { get; set; }

    /// <summary>Gets or sets the infraction name (e.g. "Tripping", "Hooking").</summary>
    public string InfractionName { get; set; } = string.Empty;

    /// <summary>Gets or sets the penalty duration in minutes.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>Gets or sets optional notes about the penalty.</summary>
    public string? Notes { get; set; }
}
