namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a real-time event broadcast when a draft pick is made.</summary>
/// <param name="DraftId">The ID of the draft in which the pick was made.</param>
/// <param name="CompletedPick">The pick that was just completed.</param>
/// <param name="DraftedPlayer">The player who was selected.</param>
/// <param name="NextPick">The next pick on the clock, if any.</param>
public record DraftPickMadeEventDto(
    int DraftId,
    DraftPickBriefDto CompletedPick,
    PlayerBriefDto DraftedPlayer,
    DraftPickSingleDto? NextPick
);