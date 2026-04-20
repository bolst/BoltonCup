using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public record UpdateDraftRequest
{
    public int TournamentId { get; set; }
    public DraftType DraftType { get; set; }
}