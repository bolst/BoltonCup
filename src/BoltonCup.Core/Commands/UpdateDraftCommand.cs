namespace BoltonCup.Core.Commands;

public sealed record UpdateDraftCommand
{
    public string? Title { get; init; }
    
    public DraftType? DraftType { get; init; }
    
    public List<DraftOrderCommandEntry>? Ordering { get; init; }
}

public sealed record DraftOrderCommandEntry(int TeamId, int Pick);