namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to update an existing goal. All fields are replaced.</summary>
public record UpdateGoalRequest
{
    /// <summary>Gets or sets the ID of the team that scored.</summary>
    public int TeamId { get; set; }

    /// <summary>Gets or sets the period number.</summary>
    public int Period { get; set; }

    /// <summary>Gets or sets the period label (e.g. "1st", "OT").</summary>
    public string PeriodLabel { get; set; } = string.Empty;

    /// <summary>Gets or sets the time remaining in the period when the goal was scored.</summary>
    public TimeSpan PeriodTimeRemaining { get; set; }

    /// <summary>Gets or sets the ID of the player who scored.</summary>
    public int GoalPlayerId { get; set; }

    /// <summary>Gets or sets the ID of the primary assist player, if any.</summary>
    public int? Assist1PlayerId { get; set; }

    /// <summary>Gets or sets the ID of the secondary assist player, if any.</summary>
    public int? Assist2PlayerId { get; set; }

    /// <summary>Gets or sets optional notes about the goal.</summary>
    public string? Notes { get; set; }
}
