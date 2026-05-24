namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to create a new draft for a tournament.</summary>
public record CreateDraftRequest
{
    /// <summary>Gets or sets the ID of the tournament this draft belongs to.</summary>
    public required int TournamentId { get; set; }

    /// <summary>Gets or sets the display title of the draft.</summary>
    public required string Title { get; set; }
}