namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of a tournament.</summary>
public sealed record TournamentBriefDto
{
    /// <summary>Gets or sets the tournament ID.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the tournament name.</summary>
    public string? Name { get; set; }
    /// <summary>Gets or sets the tournament start date.</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>Gets or sets the tournament end date.</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>Gets or sets the ID of the team that won the tournament.</summary>
    public int? WinningTeamId { get; set; }
    /// <summary>Gets or sets whether the tournament is currently active.</summary>
    public bool IsActive { get; set; }
    /// <summary>Gets or sets whether registration is currently open.</summary>
    public bool IsRegistrationOpen { get; set; }
    /// <summary>Gets or sets whether player trading is currently open.</summary>
    public bool IsTradingOpen { get; set; }
    /// <summary>Gets or sets the URL of the tournament logo.</summary>
    public string? Logo { get; set; }
}