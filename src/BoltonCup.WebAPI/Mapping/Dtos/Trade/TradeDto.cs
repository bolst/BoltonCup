using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a player trade between two teams in a tournament.</summary>
public record TradeDto
{
    /// <summary>Gets the trade ID.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the tournament this trade belongs to.</summary>
    public required int TournamentId { get; init; }
    /// <summary>Gets the team that proposed the trade.</summary>
    public required TeamBriefDto ProposingTeam { get; init; }
    /// <summary>Gets the team that received the trade proposal.</summary>
    public required TeamBriefDto ReceivingTeam { get; init; }
    /// <summary>Gets the current status of the trade.</summary>
    public required TradeStatus Status { get; init; }
    /// <summary>Gets the optional note attached to the trade.</summary>
    public string? Note { get; init; }
    /// <summary>Gets when the trade was created.</summary>
    public DateTime CreatedAt { get; init; }
    /// <summary>Gets when the receiving GM responded (accepted/declined).</summary>
    public DateTime? RespondedAt { get; init; }
    /// <summary>Gets when an admin resolved the trade (approved/cancelled).</summary>
    public DateTime? ResolvedAt { get; init; }
    /// <summary>Gets the players the proposing team is sending to the receiving team.</summary>
    public required IReadOnlyList<TradePlayerDto> PlayersFromProposing { get; init; }
    /// <summary>Gets the players the receiving team is sending to the proposing team.</summary>
    public required IReadOnlyList<TradePlayerDto> PlayersFromReceiving { get; init; }

    /// <summary>Gets whether the current viewer can accept this trade.</summary>
    public bool CanAccept { get; init; }
    /// <summary>Gets whether the current viewer can decline this trade.</summary>
    public bool CanDecline { get; init; }
    /// <summary>Gets whether the current viewer can cancel this trade.</summary>
    public bool CanCancel { get; init; }
    /// <summary>Gets whether the current viewer can approve this trade.</summary>
    public bool CanApprove { get; init; }
}

/// <summary>The viewing user's identity used to compute which actions they may take on a trade.</summary>
public sealed record TradeViewerContext(int? AccountId, bool IsAdmin);

/// <summary>A single player line item within a trade, including movement direction.</summary>
public record TradePlayerDto
{
    /// <summary>Gets the player being moved.</summary>
    public required PlayerBriefDto Player { get; init; }
    /// <summary>Gets the team the player is moving from.</summary>
    public required int FromTeamId { get; init; }
    /// <summary>Gets the team the player is moving to.</summary>
    public required int ToTeamId { get; init; }
}
