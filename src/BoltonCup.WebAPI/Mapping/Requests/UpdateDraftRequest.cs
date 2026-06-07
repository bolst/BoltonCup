using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to update an existing draft's settings.</summary>
public record UpdateDraftRequest
{
    /// <summary>Gets or sets the updated display title for the draft.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the updated draft type.</summary>
    public DraftType? DraftType { get; set; }

    /// <summary>Gets or sets the updated pick ordering for each team.</summary>
    public List<DraftOrderingRequestEntry>? Ordering { get; set; }

    /// <summary>Gets or sets whether the draft should be publicly visible.</summary>
    public bool? IsVisible { get; set; }
}

/// <summary>Specifies a team's pick position in the draft order.</summary>
/// <param name="TeamId">The team ID.</param>
/// <param name="Pick">The pick number assigned to the team.</param>
public sealed record DraftOrderingRequestEntry(int TeamId, int Pick);