using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public record UpdateDraftRequest
{
    public DraftType DraftType { get; set; }
    public DraftStatus DraftStatus { get; set; }
}