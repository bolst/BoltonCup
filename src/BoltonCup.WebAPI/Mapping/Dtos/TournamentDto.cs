namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a tournament.</summary>
public record TournamentDto
{
    /// <summary>Gets or sets the tournament ID.</summary>
    public required int Id { get; set; }
    /// <summary>Gets or sets the tournament name.</summary>
    public required string Name { get; set; }
    /// <summary>Gets or sets the URL of the tournament logo.</summary>
    public string? Logo { get; set; }
    /// <summary>Gets or sets the URL of the tournament background image.</summary>
    public string? BackgroundImage { get; set; }
    /// <summary>Gets or sets the tournament start date.</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>Gets or sets the tournament end date.</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>Gets or sets the ID of the team that won the tournament.</summary>
    public int? WinningTeamId { get; set; }
    /// <summary>Gets or sets whether the tournament is currently active.</summary>
    public required bool IsActive { get; set; }
    /// <summary>Gets or sets whether registration is currently open.</summary>
    public required bool IsRegistrationOpen { get; set; }
    /// <summary>Gets or sets whether payment is currently open.</summary>
    public required bool IsPaymentOpen { get; set; }
    /// <summary>Gets or sets whether the player info form is currently open.</summary>
    public required bool IsPlayerInfoOpen { get; set; }
    /// <summary>Gets or sets whether player trading is currently open.</summary>
    public required bool IsTradingOpen { get; set; }
    /// <summary>Gets or sets the maximum number of skaters allowed to register.</summary>
    public int? SkaterLimit { get; set; }
    /// <summary>Gets or sets the maximum number of goalies allowed to register.</summary>
    public int? GoalieLimit { get; set; }
    /// <summary>Gets or sets the gallery associated with this tournament.</summary>
    public GalleryBriefDto? Gallery { get; set; }
}