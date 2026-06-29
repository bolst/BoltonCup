using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a draft.</summary>
public record DraftDto
{
    /// <summary>Gets or sets the draft ID.</summary>
    public required int Id { get; set; }
    /// <summary>Gets or sets the draft title.</summary>
    public required string? Title { get; set; }
    /// <summary>Gets or sets the type of draft.</summary>
    public required DraftType Type { get; set; }
    /// <summary>Gets or sets the current status of the draft.</summary>
    public required DraftStatus Status { get; set; }
    /// <summary>Gets or sets the tournament this draft belongs to.</summary>
    public required TournamentBriefDto Tournament { get; set; }
    /// <summary>Gets or sets the display name of the account that created (owns) the draft.</summary>
    public string? CreatedByName { get; set; }
    /// <summary>Gets or sets whether this draft is publicly visible.</summary>
    public bool IsVisible { get; set; }
    /// <summary>Gets or sets the number of rounds in the draft.</summary>
    public int Rounds { get; set; }
    /// <summary>Gets or sets the number of teams in the draft.</summary>
    public int Teams { get; set; }
    /// <summary>Gets or sets the number of seconds each team has to make a pick.</summary>
    public int SecondsPerPick { get; set; }
}