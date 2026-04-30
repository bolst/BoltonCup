using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public record UpdateDraftRequest
{
    public string? Title { get; set; }
    
    public DraftType? DraftType { get; set; }
    
    public List<DraftOrderingRequestEntry>? Ordering { get; set; }
}

public sealed record DraftOrderingRequestEntry(int TeamId, int Pick);