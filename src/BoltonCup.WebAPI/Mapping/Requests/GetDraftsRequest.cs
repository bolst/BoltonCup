using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of drafts with optional filters.</summary>
public record GetDraftsRequest : RequestBase
{
    /// <summary>Gets or sets an optional tournament ID to filter drafts by.</summary>
    public int? TournamentId { get; set; }

    /// <summary>Gets or sets an optional status to filter drafts by.</summary>
    public DraftStatus? Status { get; set; }
}