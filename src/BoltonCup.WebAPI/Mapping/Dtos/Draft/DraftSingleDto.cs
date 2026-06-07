namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single draft, including pick order and picks by round.</summary>
public record DraftSingleDto : DraftDto
{
    /// <summary>Gets or sets the ordered list of teams in the draft pick order.</summary>
    public required IOrderedEnumerable<DraftPickOrderDto> PickOrder { get; set; }
    /// <summary>Gets or sets the draft picks grouped and ordered by round.</summary>
    public required IOrderedEnumerable<RoundDraftPicks> DraftPicksByRound { get; set; }
    /// <summary>Gets or sets whether the current user can edit this draft.</summary>
    public bool CanEditDraft { get; set; }
    /// <summary>Gets or sets whether the current user can manage this draft (admin or draft owner).</summary>
    public bool CanManageDraft { get; set; }
}

/// <summary>Represents the ordered picks for a single draft round.</summary>
/// <param name="Round">The round number.</param>
/// <param name="Picks">The ordered picks within this round.</param>
public sealed record RoundDraftPicks(int Round, IOrderedEnumerable<DraftPickDto> Picks);