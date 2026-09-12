namespace BoltonCup.Core.Commands;

public sealed record UpdateDraftCommand
{
    public string? Title { get; init; }

    public DraftType? DraftType { get; init; }

    public List<DraftOrderCommandEntry>? Ordering { get; init; }

    public bool? IsVisible { get; init; }

    public int? SecondsPerPick { get; init; }

    public List<DraftAutoPickEntry>? AutoPickSettings { get; init; }
}

public sealed record DraftOrderCommandEntry(int TeamId, int Pick);

public sealed record DraftAutoPickEntry(int TeamId, bool AutoPick);