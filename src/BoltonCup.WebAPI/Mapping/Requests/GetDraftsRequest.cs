using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public record GetDraftsRequest : RequestBase
{
    public int? TournamentId { get; set; }
    public DraftStatus? Status { get; set; }
}