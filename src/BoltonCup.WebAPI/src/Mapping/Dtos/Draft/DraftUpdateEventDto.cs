namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a real-time event broadcast when the draft state changes.</summary>
/// <param name="Draft">The current full state of the draft.</param>
/// <param name="NextPick">The next pick on the clock, if any.</param>
public record DraftUpdateEventDto(
    DraftSingleDto Draft,
    DraftPickSingleDto? NextPick
);