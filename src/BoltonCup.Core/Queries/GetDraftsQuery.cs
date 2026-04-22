using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetDraftsQuery : QueryBase
{
    public int? TournamentId { get; set; }
    public DraftStatus? Status { get; set; }
}