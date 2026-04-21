namespace BoltonCup.WebAPI.Mapping;

public sealed record UpdateDraftOrderingRequest
{
    public required List<DraftOrderingRequestEntry> Ordering { get; set; }
}

public sealed record DraftOrderingRequestEntry(int TeamId, int Pick);